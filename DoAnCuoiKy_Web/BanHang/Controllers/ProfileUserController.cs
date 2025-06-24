using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using BanHang.Models; // Đảm bảo namespace này đúng
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization; // Thêm để yêu cầu đăng nhập

namespace BanHang.Controllers
{
    [Authorize] // Yêu cầu người dùng phải đăng nhập để truy cập controller này
    public class ProfileUserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ProfileUserController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Trang chính của Profile User, hiển thị các lựa chọn
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User); // Lấy thông tin người dùng hiện tại
            if (user == null)
            {
                return NotFound("Không tìm thấy thông tin người dùng.");
            }

            ViewBag.UserName = user.UserName; // Hoặc user.Email, hoặc một trường tên riêng nếu có
            // Bạn có thể thêm các thông tin khác vào ViewBag nếu cần
            // Ví dụ: số lượng sản phẩm đã đăng (nếu có chức năng đó)

            return View(); // Trả về view Index.cshtml (sẽ tạo ở dưới)
        }

        // Trang hiển thị lịch sử đơn hàng (đổi tên từ Profile cũ để rõ ràng hơn)
        public async Task<IActionResult> OrderHistory()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge(); // Hoặc RedirectToAction("Login", "Account");
            }

            // Lấy các đơn hàng chỉ của người dùng hiện tại
            var orders = await _context.Orders
                .Where(o => o.UserId == currentUser.Id) // Lọc theo UserId
                .Include(o => o.User) // Vẫn include User nếu cần hiển thị tên/email trong bảng
                .OrderByDescending(o => o.OrderDate) // Sắp xếp đơn hàng mới nhất lên đầu
                .ToListAsync();

            return View(orders); // Trả về view OrderHistory.cshtml (sẽ là view Profile.cshtml cũ của bạn)
        }

        // Chi tiết đơn hàng (giữ nguyên)
        public async Task<IActionResult> ProfileDetails(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == currentUser.Id); // Đảm bảo chỉ xem đơn hàng của mình

            if (order == null)
                return NotFound();

            return View(order);
        }

        // Hủy đơn hàng (giữ nguyên, nhưng thêm kiểm tra quyền)
        [HttpPost]
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == currentUser.Id); // Đảm bảo hủy đơn của mình
            if (order == null)
            {
                return NotFound();
            }

            if (order.Status != "Chờ xác nhận")
            {
                TempData["ErrorMessage"] = "Không thể hủy đơn hàng đã được xử lý hoặc không ở trạng thái chờ xác nhận.";
                return RedirectToAction("OrderHistory");
            }

            order.Status = "Đã hủy";
            // Cập nhật lại số lượng sản phẩm nếu cần (phức tạp hơn, tùy logic nghiệp vụ)
            _context.Orders.Update(order); // Đánh dấu là đã thay đổi
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã hủy đơn hàng thành công.";
            return RedirectToAction("OrderHistory");
        }
    }
}