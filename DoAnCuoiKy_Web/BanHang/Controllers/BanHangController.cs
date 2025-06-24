
using BanHang.Models;
using BanHang.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace BanHang.Controllers
{
    public class BanHangController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public BanHangController(ApplicationDbContext context, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // Hiển thị trang đăng nhập
        public IActionResult Login()
        {
            return View();
        }

        // Xử lý đăng nhập
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null && await _userManager.CheckPasswordAsync(user, password))
            {
                await _signInManager.SignInAsync(user, isPersistent: false);

                // Kiểm tra vai trò của user
                var roles = await _userManager.GetRolesAsync(user);
                Console.WriteLine($"User {user.Email} có các vai trò: {string.Join(", ", roles)}");

                if (roles.Contains("Admin"))
                {
                    return RedirectToAction("AdminDashboard", "Admin");
                }

                return RedirectToAction("MainPage", "BanHang");
            }

            ViewBag.ErrorMessage = "Sai tài khoản hoặc mật khẩu!";
            return View();
        }


        // Đăng xuất
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        // Hiển thị trang chính của User (Chỉ User đăng nhập mới xem được)
        [Authorize]
        public IActionResult MainPage()
        {
            return View();
        }

        // Các trang không yêu cầu đăng nhập
        public IActionResult About() => View("About");
        public IActionResult Shop(int? categoryId, string search)
        {
            // Lấy danh sách danh mục (hiển thị ở phần menu hoặc dropdown nếu có)
            var categories = _context.Categories.ToList();
            ViewBag.Categories = categories;

            // Giữ lại thông tin lọc hiện tại để View hiển thị đúng
            ViewBag.SelectedCategory = categoryId;
            ViewBag.SearchKeyword = search;

            // Lấy danh sách sản phẩm kèm thông tin danh mục
            var products = _context.Products.Include(p => p.Category).AsQueryable();

            // Lọc theo danh mục nếu có chọn
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                products = products.Where(p => p.CategoryId == categoryId.Value);
            }

            // Lọc theo từ khóa tìm kiếm nếu có nhập
            if (!string.IsNullOrEmpty(search))
            {
                products = products.Where(p => p.Name.Contains(search));
            }

            // Trả dữ liệu về View
            return View("Shop", products.ToList());
        }

        


        public IActionResult Contact() => View("Contact");
        public IActionResult Blog() => View("Blog");


        public IActionResult Wishlist()
        {
            var favorites = _context.FavoriteProducts
                            .Include(f => f.Product)
                            .ToList();
            return View(favorites);
        }
        public async Task<IActionResult> ProductSingle(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // SỬA LẠI DÒNG NÀY
            var product = await _context.Products
     .Include(p => p.Category)
     .Include(p => p.User)
     .Include(p => p.Images) // ✅ THÊM DÒNG NÀY
     .FirstOrDefaultAsync(m => m.Id == id);


            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }
        public async Task<IActionResult> Checkout()
        {
            // Lấy người dùng hiện tại
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            // Lấy các sản phẩm trong giỏ hàng của người dùng
            var cartItems = await _context.Carts
                .Include(ci => ci.Product)
                .Where(ci => ci.UserId == user.Id)
                .ToListAsync();

            // Nếu giỏ hàng trống, chuyển đến trang giỏ hàng
            if (!cartItems.Any())
            {
                return RedirectToAction("Cart");
            }

            // Lấy các địa chỉ của người dùng (tất cả các địa chỉ trong bảng Locations)
            var userLocations = await _context.Locations
                .Where(l => l.UserId == user.Id)
                .ToListAsync();

            // Nếu người dùng chưa có địa chỉ, hướng dẫn họ thêm địa chỉ
            if (!userLocations.Any())
            {
                TempData["ErrorMessage"] = "Bạn cần thêm địa chỉ giao hàng trước khi thanh toán!";
                return RedirectToAction("AddLocation");
            }

            // Tạo model Checkout
            var checkoutModel = new Checkout
            {
                CartItems = cartItems.Select(c => new CartItem
                {
                    ProductName = c.Product.Name,
                    Quantity = c.Quantity,
                    UnitPrice = c.Product.Price,
                    TotalPrice = c.Quantity * c.Product.Price
                }).ToList(),
                TotalCartPrice = cartItems.Sum(c => c.Quantity * c.Product.Price), // Tổng giá trị giỏ hàng
                Locations = userLocations, // Các địa chỉ của người dùng
            };

            // Trả về Checkout view với dữ liệu đã chuẩn bị
            return View(checkoutModel);
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CheckoutConfirm()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login");

            // Lấy giỏ hàng
            var cartItems = await _context.Carts
                .Include(c => c.Product)
                .Where(c => c.UserId == user.Id)
                .ToListAsync();

            if (!cartItems.Any())
                return RedirectToAction("Cart");

            // Lấy địa chỉ gần nhất của user
            var location = await _context.Locations
                .Where(l => l.UserId == user.Id)
                .OrderByDescending(l => l.Id)
                .FirstOrDefaultAsync();

            // Tạo đơn hàng mới
            var order = new Order
            {
                UserId = user.Id,
                OrderDate = DateTime.Now,
                TotalPrice = cartItems.Sum(c => c.Quantity * c.Product.Price),
                Status = "Chờ xác nhận",
                LocationId = location?.Id
            };
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Thêm chi tiết đơn hàng
            foreach (var item in cartItems)
            {
                var orderDetail = new OrderDetail
                {
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.Product.Price
                };
                _context.OrderDetails.Add(orderDetail);
            }

            // Xóa giỏ hàng
            _context.Carts.RemoveRange(cartItems);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đặt hàng thành công!";
            return RedirectToAction("MainPage");
        }



        public async Task<IActionResult> Cart()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            var cartItems = _context.Carts
                .Include(ci => ci.Product)
                .Where(ci => ci.UserId == user.Id)
                .ToList();

            return View(cartItems);
        }
		[HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return NotFound();
            }

            if (product.StockQuantity == 0)
            {
                return Json(new { success = false, message = "Sản phẩm đã hết hàng." });
            }

            var cartItem = await _context.Carts
                .FirstOrDefaultAsync(c => c.ProductId == productId && c.UserId == user.Id);

            if (cartItem != null)
            {
                if (cartItem.Quantity >= product.StockQuantity)
                {
                    return Json(new { success = false, message = "Số lượng trong giỏ hàng đã đủ, không thể thêm nữa." });
                }
                else
                {
                    cartItem.Quantity += quantity;
                    if (cartItem.Quantity > product.StockQuantity)
                    {
                        cartItem.Quantity = product.StockQuantity; 
                    }
                    cartItem.Price = (decimal)(cartItem.Quantity * product.Price);
                    _context.Carts.Update(cartItem);
                }
            }
            else
            {
                if (quantity > product.StockQuantity)
                {
                    quantity = product.StockQuantity; 
                }

                var newCartItem = new Cart
                {
                    UserId = user.Id,
                    ProductId = productId,
                    Quantity = quantity,
                    Price = (decimal)(product.Price * quantity),
                    DateAdded = DateTime.Now
                };
                await _context.Carts.AddAsync(newCartItem);
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true});
        }



        [HttpPost]
		public async Task<IActionResult> UpdateCart(int id, int quantity)
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null)
			{
				return Json(new { success = false }); // Trả về JSON nếu người dùng chưa đăng nhập
			}

			var cartItem = await _context.Carts
				.Include(c => c.Product) // Đảm bảo thông tin sản phẩm được lấy
				.FirstOrDefaultAsync(c => c.Id == id && c.UserId == user.Id);

			if (cartItem != null && quantity > 0)
			{
				var product = cartItem.Product;

				// Kiểm tra nếu số lượng vượt quá tồn kho
				if (quantity > product.StockQuantity)
				{
					return Json(new { success = false, message = "Số lượng vượt quá tồn kho!", disablePlusButton = true });
				}

				cartItem.Quantity = quantity;
				cartItem.Price = cartItem.Product.Price * quantity; // Tính lại giá trị Price
				_context.Carts.Update(cartItem);
				await _context.SaveChangesAsync();

				// Trả về tổng tiền đã cập nhật và trạng thái của nút cộng
				var newTotalPrice = (cartItem.Price).ToString("C0", new CultureInfo("vi-VN"));
				bool disablePlusButton = quantity >= product.StockQuantity; // Nếu số lượng bằng hoặc vượt qua tồn kho thì vô hiệu hóa nút cộng
				return Json(new { success = true, newTotalPrice, disablePlusButton });
			}

			return Json(new { success = false });
		}


		// GET: AddLocation
		public IActionResult AddLocation()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddLocation(Location model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }
            var existingLocation = await _context.Locations
                .FirstOrDefaultAsync(l => l.UserId == user.Id && l.StreetAddress == model.StreetAddress);

            if (existingLocation != null)
            {
                existingLocation.Ward = model.Ward;
                existingLocation.District = model.District;
                existingLocation.Province = model.Province;
                existingLocation.Region = model.Region;

                _context.Locations.Update(existingLocation);
            }
            else
            {
                model.UserId = user.Id;
                _context.Locations.Add(model);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("Checkout");
        }


        public async Task<IActionResult> TestSave()
        {
            var testUserId = "c90bf338-18dc-4533-98a2-bdf7044cdc24"; // copy từ DB, đảm bảo tồn tại

            var testLocation = new Location
            {
                UserId = testUserId,
                StreetAddress = "123 Test St",
                Ward = "Phường Test",
                District = "Quận Test",
                Province = "Thành phố Test",
                Region = "Miền Nam"
            };

            _context.Locations.Add(testLocation);
            await _context.SaveChangesAsync();

            return Content("Đã lưu location cho user id: " + testUserId);
        }
        public async Task<IActionResult> ProfileDetail(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                // Nếu không có userId, trả về lỗi không tìm thấy
                return NotFound("Không tìm thấy người dùng.");
            }

            // Dùng _userManager đã có sẵn để tìm thông tin người dùng
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                // Nếu không tìm thấy user trong CSDL, báo lỗi
                return NotFound("Người dùng này không tồn tại.");
            }

            // Lấy tất cả sản phẩm mà người dùng này đã đăng
            // Sử dụng _context để truy vấn trực tiếp, giống các action khác trong controller này
            var products = await _context.Products
                                         .Where(p => p.UserId == userId)
                                         .Include(p => p.Category) // Lấy thông tin danh mục
                                         .OrderByDescending(p => p.Id)
                                         .ToListAsync();

            // Tạo ViewModel và gán dữ liệu đã lấy được
            var viewModel = new UserProfileViewModel
            {
                UserProfile = user,
                UserProducts = products
            };

            // Trả về View ProfileDetail với ViewModel đã chuẩn bị
            return View("~/Views/BanHang/ProfileDetail.cshtml", viewModel);
        }

    }

 }
