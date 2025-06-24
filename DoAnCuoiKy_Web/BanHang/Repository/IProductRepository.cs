// IProductRepository.cs
using BanHang.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IProductRepository
{
    // Lấy tất cả sản phẩm, thường dùng cho trang Shop của khách hàng
    Task<IEnumerable<Product>> GetAllAsync();

    // Lấy tất cả sản phẩm kèm theo thông tin chi tiết (User, Category), dùng cho Admin
    Task<IEnumerable<Product>> GetAllWithDetailsAsync();

    // Lấy sản phẩm theo ID người dùng, dùng để hiển thị "Sản phẩm của tôi"
    Task<IEnumerable<Product>> GetByUserIdAsync(string userId);

    // Lấy một sản phẩm theo ID (ít thông tin)
    Task<Product?> GetByIdAsync(int id);

    // Lấy một sản phẩm theo ID kèm thông tin chi tiết (User, Category)
    Task<Product?> GetByIdWithDetailsAsync(int id);

    // Lấy các sản phẩm theo danh mục
    Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId);

    // Thêm sản phẩm mới
    Task AddAsync(Product product);

    // Cập nhật thông tin sản phẩm
    Task UpdateAsync(Product product);

    // Xóa sản phẩm
    Task DeleteAsync(int id);

    // Cập nhật số lượng tồn kho (giữ lại vì model vẫn còn StockQuantity)
    Task UpdateStockAsync(int productId, int quantitySold);
    Task UpdateApprovalStatusAsync(int id, ProductApprovalStatus status);
    void AddProductImage(ProductImage image);


}