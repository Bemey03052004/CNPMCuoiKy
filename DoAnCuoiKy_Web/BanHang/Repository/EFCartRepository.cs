// EFCartRepository.cs
using BanHang.Models;
using Microsoft.EntityFrameworkCore;

namespace BanHang.Repository
{
    public class EFCartRepository : ICartRepository
    {
        private readonly ApplicationDbContext _context;

        public EFCartRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AddToCartAsync(string userId, int productId, int quantity)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null || quantity <= 0)
            {
                return false;
            }

            if (product.StockQuantity < quantity)
            {
                return false; // Không đủ hàng
            }

            var cartItem = await _context.Carts
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);

            if (cartItem != null)
            {
                if (product.StockQuantity < (cartItem.Quantity + quantity))
                {
                    // return false; // Không đủ hàng cho tổng số lượng mới
                    // Hoặc giới hạn số lượng thêm vào:
                    int canAdd = product.StockQuantity - cartItem.Quantity;
                    if(canAdd <= 0) return false; // Không thể thêm nữa
                    cartItem.Quantity += canAdd;
                }
                else
                {
                    cartItem.Quantity += quantity;
                }
                cartItem.Price = product.Price;
            }
            else
            {
                cartItem = new Cart
                {
                    UserId = userId,
                    ProductId = productId,
                    Quantity = quantity,
                    Price = product.Price,
                    DateAdded = DateTime.UtcNow
                };
                await _context.Carts.AddAsync(cartItem);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Cart>> GetCartItemsAsync(string userId)
        {
            return await _context.Carts
                .Where(c => c.UserId == userId)
                .Include(c => c.Product)
                    .ThenInclude(p => p!.Category)
                .OrderByDescending(c => c.DateAdded)
                .ToListAsync();
        }

        public async Task<Cart?> GetCartItemByIdAsync(int cartItemId)
        {
            return await _context.Carts
                                 .Include(c => c.Product)
                                 .FirstOrDefaultAsync(c => c.Id == cartItemId);
        }

        public async Task<bool> UpdateCartItemQuantityAsync(int cartItemId, int quantity, string userId)
        {
            var cartItem = await _context.Carts
                                         .Include(c => c.Product)
                                         .FirstOrDefaultAsync(c => c.Id == cartItemId && c.UserId == userId);

            if (cartItem == null) return false;

            if (quantity <= 0)
            {
                _context.Carts.Remove(cartItem);
            }
            else
            {
                if (cartItem.Product != null && cartItem.Product.StockQuantity < quantity)
                {
                    // return false; // Không đủ hàng
                    // Hoặc cập nhật tới số lượng tối đa
                    cartItem.Quantity = cartItem.Product.StockQuantity;
                    if (cartItem.Quantity <= 0) _context.Carts.Remove(cartItem);
                }
                else
                {
                    cartItem.Quantity = quantity;
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveCartItemByIdAsync(int cartItemId, string userId)
        {
            var cartItem = await _context.Carts
                                         .FirstOrDefaultAsync(c => c.Id == cartItemId && c.UserId == userId);
            if (cartItem == null) return false;

            _context.Carts.Remove(cartItem);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ClearCartAsync(string userId)
        {
            var cartItems = _context.Carts.Where(c => c.UserId == userId);
            if (await cartItems.AnyAsync())
            {
                _context.Carts.RemoveRange(cartItems);
                await _context.SaveChangesAsync();
            }
            return true;
        }

        public async Task<decimal> GetCartTotalAsync(string userId)
        {
            var items = await _context.Carts
                .Where(c => c.UserId == userId)
                .ToListAsync();

            return items.Sum(c => c.Quantity * c.Price);
        }

        public async Task<int> GetCartItemCountAsync(string userId)
        {
            return await _context.Carts
                .Where(c => c.UserId == userId)
                .CountAsync();
        }
    }
}