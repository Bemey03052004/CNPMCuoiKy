using BanHang.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BanHang.Controllers
{
    [Authorize]
    public class FavoriteController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public FavoriteController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Thêm sản phẩm vào yêu thích
        [HttpPost]
        public async Task<IActionResult> Add(int productId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // Kiểm tra đã tồn tại trong yêu thích chưa
            var existing = await _context.FavoriteProducts
                .FirstOrDefaultAsync(f => f.UserId == user.Id && f.ProductId == productId);

            if (existing == null)
            {
                var favorite = new FavoriteProduct
                {
                    ProductId = productId,
                    UserId = user.Id
                };

                _context.FavoriteProducts.Add(favorite);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("shop", "BanHang");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Remove(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var favorite = await _context.FavoriteProducts
                .FirstOrDefaultAsync(f => f.Id == id && f.UserId == user.Id);

            if (favorite == null)
                return NotFound();

            _context.FavoriteProducts.Remove(favorite);
            await _context.SaveChangesAsync();

            return RedirectToAction("Wishlist","BanHang");
        }


    }

}
