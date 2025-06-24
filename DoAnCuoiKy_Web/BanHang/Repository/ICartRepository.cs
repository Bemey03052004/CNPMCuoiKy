// ICartRepository.cs
using BanHang.Models;

namespace BanHang.Repository
{
    public interface ICartRepository
    {
        Task<bool> AddToCartAsync(string userId, int productId, int quantity);
        Task<IEnumerable<Cart>> GetCartItemsAsync(string userId);
        Task<Cart?> GetCartItemByIdAsync(int cartItemId);
        Task<bool> UpdateCartItemQuantityAsync(int cartItemId, int quantity, string userId);
        Task<bool> RemoveCartItemByIdAsync(int cartItemId, string userId);
        Task<bool> ClearCartAsync(string userId);
        Task<decimal> GetCartTotalAsync(string userId);
        Task<int> GetCartItemCountAsync(string userId);
    }
}