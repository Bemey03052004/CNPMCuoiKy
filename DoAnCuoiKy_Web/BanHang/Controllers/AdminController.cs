using BanHang.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BanHang.Controllers
{
    [Authorize(Roles = "Admin")] // Chỉ Admin mới truy cập được
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(ApplicationDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public IActionResult AdminDashboard()
        {
            return View();
        }
        public IActionResult Product()
        {
            return RedirectToAction("Index", "Product");
        }
        // 🛒 Quản lý đơn hàng: Danh sách đơn hàng
        public async Task<IActionResult> Orders()
        {
            var orders = await _context.Orders
                .Include(o => o.User) // Lấy thông tin User từ bảng AspNetUsers
                .ToListAsync();


            return View(orders);
        }
        // 📜 Chi tiết đơn hàng
        public async Task<IActionResult> OrderDetails(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product) // Lấy thông tin sản phẩm
                .Include(o => o.User) // Lấy thông tin khách hàng
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound();

            return View(order);
        }


        public async Task<IActionResult> User()
        {
            var users = await _userManager.Users.ToListAsync();
            var userList = new List<UserViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user); // 🔥 Lấy danh sách Role của User
                userList.Add(new UserViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Role = roles.FirstOrDefault() ?? "User" // 👈 Mặc định nếu không có Role nào
                });
            }

            return View(userList);
        }
        [Route("Admin/DetailsUser/{id}")]
        public async Task<IActionResult> UserDetails(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user); // 🔥 Lấy danh sách Role của User

            var userDetails = new UserViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Role = roles.FirstOrDefault() ?? "User"
            };

            return View(userDetails);
        }

        // Trong AdminController.cs

        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // === THÊM ĐOẠN CODE NÀY VÀO ===
            // 1. Tìm tất cả sản phẩm thuộc về người dùng này
            var userProducts = await _context.Products.Where(p => p.UserId == id).ToListAsync();

            // 2. Nếu có sản phẩm, xóa chúng đi
            if (userProducts.Any())
            {
                _context.Products.RemoveRange(userProducts);
                await _context.SaveChangesAsync(); // Lưu thay đổi (xóa sản phẩm)
            }
            // === KẾT THÚC ĐOẠN CODE THÊM ===


            // 3. Bây giờ mới xóa người dùng
            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                // ... chuyển hướng thành công
            }

            // ... xử lý lỗi nếu xóa user không thành công

            return RedirectToAction("UserManagement"); // Hoặc tên action quản lý user của bạn
        }

        //doanh thu theo ngày tháng năm

        public IActionResult RevenueStatistics(string filterType, int? year, int? month, int? day)
        {
            var query = _context.Orders
                .Where(o => o.Status == "Hoàn Thành") // ✅ Chỉ lấy đơn hàng đã hoàn thành
                .AsQueryable();

            if (filterType == "day" && year.HasValue && month.HasValue && day.HasValue)
            {
                query = query.Where(o => o.OrderDate.Year == year && o.OrderDate.Month == month && o.OrderDate.Day == day);
            }
            else if (filterType == "month" && year.HasValue && month.HasValue)
            {
                query = query.Where(o => o.OrderDate.Year == year && o.OrderDate.Month == month);
            }
            else if (filterType == "year" && year.HasValue)
            {
                query = query.Where(o => o.OrderDate.Year == year);
            }

            var revenue = query.Sum(o => o.TotalPrice);
            var orderCount = query.Count();

            var model = new RevenueStatisticsViewModel
            {
                FilterType = filterType,
                Year = year,
                Month = month,
                Day = day,
                TotalRevenue = revenue,
                OrderCount = orderCount
            };

            return View(model);
        }

  //      // Cập nhật tồn kho khi đơn hàng hoàn thành
  //      public async Task<IActionResult> CompleteOrder(int id)
  //      {
  //          var order = await _context.Orders
  //              .Include(o => o.OrderDetails)
  //              .ThenInclude(od => od.Product)
  //              .FirstOrDefaultAsync(o => o.Id == id);

  //          if (order == null)
  //              return NotFound();

  //          if (order.Status != "Chờ xác nhận")
  //          {
  //              // Nếu trạng thái đơn hàng không phải "Pending", không thực hiện cập nhật
  //              return BadRequest("Chỉ đơn hàng đang ở trạng thái 'Chờ xác nhận' mới có thể hoàn thành.");
  //          }

  //          // Giảm số lượng tồn kho cho các sản phẩm trong đơn hàng
  //          foreach (var orderDetail in order.OrderDetails)
  //          {
  //              var product = orderDetail.Product;

  //              // Kiểm tra số lượng tồn kho, tránh để tồn kho âm
  //              if (product.StockQuantity < orderDetail.Quantity)
  //              {
  //                  return BadRequest($"Không đủ hàng tồn kho cho sản phẩm: {product.Name}");
  //              }

  //              // Giảm số lượng tồn kho cho sản phẩm
  //              product.StockQuantity -= orderDetail.Quantity;
  //          }

  //          // Cập nhật trạng thái đơn hàng thành "Completed"
  //          order.Status = "Hoàn thành";
  //          await _context.SaveChangesAsync();

  //          return RedirectToAction("Orders");
  //      }
  //      // Cập nhật tồn kho khi đơn hàng bị hủy
  //      public async Task<IActionResult> CancelOrder(int id)
  //      {
  //          var order = await _context.Orders
  //              .Include(o => o.OrderDetails)
  //              .ThenInclude(od => od.Product)
  //              .FirstOrDefaultAsync(o => o.Id == id);

  //          if (order == null)
  //              return NotFound();

  //          if (order.Status == "Đã hủy")
  //          {
  //              // Nếu đơn hàng đã bị hủy, không thực hiện lại cập nhật
  //              return BadRequest("Đơn hàng đã bị hủy.");
  //          }

  //          // Cập nhật lại số lượng tồn kho cho các sản phẩm trong đơn hàng
  //          foreach (var orderDetail in order.OrderDetails)
  //          {
  //              var product = orderDetail.Product;

  //              // Cộng lại số lượng sản phẩm vào kho
  //              product.StockQuantity += orderDetail.Quantity;
  //          }

  //          // Cập nhật trạng thái đơn hàng thành "Cancelled"
  //          order.Status = "Đã hủy";
  //          await _context.SaveChangesAsync();

  //          return RedirectToAction("Orders");
  //      }

		//[HttpPost]
		//public async Task<IActionResult> UpdateStatus(int id, string status)
		//{
		//	var order = await _context.Orders
		//		.Include(o => o.OrderDetails)
		//		.ThenInclude(od => od.Product) // Lấy thông tin sản phẩm
		//		.FirstOrDefaultAsync(o => o.Id == id);

		//	if (order == null)
		//		return NotFound();

		//	string oldStatus = order.Status;

		//	// Cập nhật trạng thái đơn hàng
		//	order.Status = status;
		//	await _context.SaveChangesAsync();

		//	// Nếu trạng thái mới là "Hoàn thành" thì trừ số lượng tồn kho
		//	if (status == "Hoàn thành")
		//	{
		//		foreach (var orderDetail in order.OrderDetails)
		//		{
		//			var product = orderDetail.Product;
		//			if (product != null)
		//			{
		//				// Trừ số lượng sản phẩm khỏi kho khi đơn hàng hoàn thành
		//				product.StockQuantity -= orderDetail.Quantity;

		//				// Kiểm tra nếu tồn kho không đủ
		//				if (product.StockQuantity < 0)
		//				{
		//					// Nếu tồn kho không đủ, không thực hiện cập nhật và trả lại thông báo lỗi
		//					product.StockQuantity += orderDetail.Quantity; // Đưa lại số lượng về ban đầu
		//					return BadRequest($"Không đủ hàng tồn kho cho sản phẩm: {product.Name}");
		//				}
		//			}
		//		}
		//	}
		//	// Nếu trạng thái mới là "Đã hủy" thì cộng lại số lượng tồn kho
		//	else if (status == "Đã hủy")
		//	{
		//		foreach (var orderDetail in order.OrderDetails)
		//		{
		//			var product = orderDetail.Product;
		//			if (product != null)
		//			{
		//				// Cộng lại số lượng sản phẩm vào kho khi đơn hàng bị hủy
		//				product.StockQuantity += orderDetail.Quantity;
		//			}
		//		}
		//	}

		//	// Lưu thay đổi vào cơ sở dữ liệu
		//	await _context.SaveChangesAsync();

		//	return RedirectToAction("Orders");
		//}

    }
}
