// CartController.cs
using BanHang.Models;
using BanHang.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq; // Cho .Sum()
using System.Security.Claims; // Cho User.FindFirstValue(ClaimTypes.NameIdentifier)
using System.Threading.Tasks;

[Authorize] // Bắt buộc đăng nhập cho tất cả các action trong controller này
public class CartController : Controller
{
    private readonly ICartRepository _cartRepository;
    private readonly IProductRepository _productRepository; // Để kiểm tra sản phẩm và tồn kho
    private readonly UserManager<IdentityUser> _userManager;

    public CartController(
        ICartRepository cartRepository,
        IProductRepository productRepository,
        UserManager<IdentityUser> userManager)
    {
        _cartRepository = cartRepository;
        _productRepository = productRepository;
        _userManager = userManager;
    }

    // GET: /Cart hoặc /Cart/Index - Hiển thị giỏ hàng
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Challenge(); // Sẽ không xảy ra do [Authorize]
        }

        var cartItems = await _cartRepository.GetCartItemsAsync(userId);
        var cartTotal = await _cartRepository.GetCartTotalAsync(userId);
        var cartItemCount = await _cartRepository.GetCartItemCountAsync(userId); // Tổng số loại sản phẩm

        ViewBag.CartTotal = cartTotal;
        ViewBag.CartItemCount = cartItems.Sum(ci => ci.Quantity); // Tổng số lượng tất cả các sản phẩm
        ViewBag.DistinctItemCount = cartItemCount; // Số loại sản phẩm khác nhau

        return View(cartItems);
    }

    // POST: /Cart/AddToCart - Thêm sản phẩm vào giỏ hàng (Thường được gọi từ trang sản phẩm)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            // Trường hợp này không nên xảy ra với [Authorize]
            return Json(new { success = false, message = "Vui lòng đăng nhập để thực hiện thao tác này." });
        }

        if (productId <= 0 || quantity <= 0)
        {
            return Json(new { success = false, message = "Thông tin sản phẩm hoặc số lượng không hợp lệ." });
        }

        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null)
        {
            return Json(new { success = false, message = "Sản phẩm không tồn tại." });
        }

        // Repository sẽ kiểm tra tồn kho chi tiết hơn khi AddToCartAsync được gọi
        var success = await _cartRepository.AddToCartAsync(userId, productId, quantity);

        if (success)
        {
            var cartItems = await _cartRepository.GetCartItemsAsync(userId);
            var totalItemsInCart = cartItems.Sum(c => c.Quantity); // Tổng số lượng các sản phẩm trong giỏ
            var distinctItemCount = await _cartRepository.GetCartItemCountAsync(userId); // Số loại sản phẩm

            return Json(new
            {
                success = true,
                message = $"Đã thêm '{product.Name}' vào giỏ hàng.",
                totalQuantityInCart = totalItemsInCart, // Tổng số lượng sản phẩm
                distinctItemCount = distinctItemCount // Số loại sản phẩm
            });
        }
        else
        {
            // Lý do thất bại có thể là không đủ tồn kho (Repository đã xử lý)
            return Json(new { success = false, message = $"Không thể thêm '{product.Name}' vào giỏ. Số lượng tồn kho có thể không đủ." });
        }
    }

    // POST: /Cart/UpdateItemQuantity - Cập nhật số lượng của một mục trong giỏ hàng
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateItemQuantity(int cartItemId, int quantity)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Json(new { success = false, message = "Vui lòng đăng nhập." });
        }

        if (cartItemId <= 0)
        {
            return Json(new { success = false, message = "Mã mục trong giỏ không hợp lệ." });
        }

        // quantity < 0 sẽ không được phép. quantity = 0 sẽ được repository xử lý là xóa.
        if (quantity < 0)
        {
            return Json(new { success = false, message = "Số lượng không thể là số âm." });
        }


        var success = await _cartRepository.UpdateCartItemQuantityAsync(cartItemId, quantity, userId);

        if (success)
        {
            var cartItem = await _cartRepository.GetCartItemByIdAsync(cartItemId); // Lấy lại item (có thể đã bị xóa nếu quantity=0)
            var totalCartPrice = await _cartRepository.GetCartTotalAsync(userId);
            var cartItemsAfterUpdate = await _cartRepository.GetCartItemsAsync(userId);
            var totalQuantityInCart = cartItemsAfterUpdate.Sum(c => c.Quantity);
            var distinctItemCount = await _cartRepository.GetCartItemCountAsync(userId);

            return Json(new
            {
                success = true,
                message = quantity > 0 ? "Đã cập nhật số lượng." : "Đã xóa sản phẩm khỏi giỏ.",
                itemRemoved = quantity <= 0, // Cho biết item có bị xóa không
                itemId = cartItemId,
                newQuantity = cartItem?.Quantity ?? 0, // Số lượng mới của item (0 nếu đã xóa)
                itemTotalPrice = cartItem != null ? (cartItem.Quantity * cartItem.Price).ToString("N0") : "0",
                cartTotalPrice = totalCartPrice.ToString("N0"),
                totalQuantityInCart = totalQuantityInCart,
                distinctItemCount = distinctItemCount
            });
        }
        else
        {
            return Json(new { success = false, message = "Không thể cập nhật số lượng. Số lượng tồn kho có thể không đủ hoặc mục hàng không tồn tại." });
        }
    }


    // POST: /Cart/RemoveItem - Xóa một mục khỏi giỏ hàng
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveItem(int cartItemId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Json(new { success = false, message = "Vui lòng đăng nhập." });
        }

        if (cartItemId <= 0)
        {
            return Json(new { success = false, message = "Mã mục trong giỏ không hợp lệ." });
        }

        var result = await _cartRepository.RemoveCartItemByIdAsync(cartItemId, userId);

        if (result)
        {
            var totalCartPrice = await _cartRepository.GetCartTotalAsync(userId);
            var cartItemsAfterRemove = await _cartRepository.GetCartItemsAsync(userId);
            var totalQuantityInCart = cartItemsAfterRemove.Sum(c => c.Quantity);
            var distinctItemCount = await _cartRepository.GetCartItemCountAsync(userId);

            return Json(new
            {
                success = true,
                message = "Đã xóa sản phẩm khỏi giỏ hàng.",
                itemId = cartItemId,
                cartTotalPrice = totalCartPrice.ToString("N0"),
                totalQuantityInCart = totalQuantityInCart,
                distinctItemCount = distinctItemCount
            });
        }
        else
        {
            return Json(new { success = false, message = "Không thể xóa sản phẩm khỏi giỏ hàng hoặc sản phẩm không tồn tại." });
        }
    }

    // POST: /Cart/ClearCart - Xóa toàn bộ giỏ hàng
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ClearCart()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            // Không nên xảy ra do [Authorize]
            return Json(new { success = false, message = "Vui lòng đăng nhập." });
        }

        await _cartRepository.ClearCartAsync(userId);
        return Json(new
        {
            success = true,
            message = "Giỏ hàng của bạn đã được dọn sạch!",
            cartTotalPrice = "0",
            totalQuantityInCart = 0,
            distinctItemCount = 0
        });
    }

    // Helper method để redirect về trang trước đó hoặc trang mặc định (nếu không dùng JSON response)
    private IActionResult RedirectToPreviousPageOrDefault(string defaultAction = "Index", string? defaultController = null)
    {
        string refererUrl = Request.Headers["Referer"].ToString();
        if (!string.IsNullOrEmpty(refererUrl) && Url.IsLocalUrl(refererUrl))
        {
            return Redirect(refererUrl);
        }
        if (defaultController == null)
        {
            defaultController = ControllerContext.ActionDescriptor.ControllerName;
        }
        return RedirectToAction(defaultAction, defaultController);
    }
}