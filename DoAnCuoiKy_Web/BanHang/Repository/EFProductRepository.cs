// EFProductRepository.cs
using BanHang.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class EFProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _context;

    public EFProductRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    // Lấy tất cả sản phẩm, bao gồm Category để hiển thị tên danh mục
    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _context.Products
                             .Include(p => p.Category)
                             .OrderByDescending(p => p.Id)
                             .ToListAsync();
    }

    // Lấy tất cả sản phẩm, bao gồm cả Category và User (cho trang quản lý của Admin)
    public async Task<IEnumerable<Product>> GetAllWithDetailsAsync()
    {
        return await _context.Products
                             .Include(p => p.Category)
                             .Include(p => p.User) // Gộp thông tin người đăng
                             .OrderByDescending(p => p.Id)
                             .ToListAsync();
    }

    // Lấy sản phẩm theo UserId, bao gồm Category và User
    public async Task<IEnumerable<Product>> GetByUserIdAsync(string userId)
    {
        return await _context.Products
                             .Where(p => p.UserId == userId)
                             .Include(p => p.Category)
                             .Include(p => p.User)
                             .OrderByDescending(p => p.Id)
                             .ToListAsync();
    }

    // Phương thức GetByIdAsync đã có sẵn và ổn
    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _context.Products
                             .Include(p => p.Category)
                             .Include(p => p.User)
                             .Include(p => p.Images) // ✅ Load ảnh phụ ở đây
                             .FirstOrDefaultAsync(p => p.Id == id);
    }

    // Phương thức này đã có sẵn và ổn
    public async Task<Product?> GetByIdWithDetailsAsync(int id)
    {
        return await _context.Products
                             .Include(p => p.Category)
                             .Include(p => p.User)
                             .FirstOrDefaultAsync(p => p.Id == id);
    }

    // Phương thức này đã có sẵn và ổn
    public async Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId)
    {
        return await _context.Products
                             .Where(p => p.CategoryId == categoryId)
                             .Include(p => p.Category)
                             .ToListAsync();
    }

    // Phương thức này đã có sẵn và ổn
    public async Task AddAsync(Product product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
    }

    // Phương thức này đã có sẵn và ổn
    public async Task UpdateAsync(Product product)
    {
        _context.Products.Update(product);
        await _context.SaveChangesAsync();
    }

    // Phương thức này đã có sẵn và ổn
    public async Task DeleteAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product != null)
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }
    }

    // Phương thức này đã có sẵn và ổn, logic không cần thay đổi
    public async Task UpdateStockAsync(int productId, int quantitySold)
    {
        var product = await _context.Products.FindAsync(productId);
        if (product != null)
        {
            if (product.StockQuantity >= quantitySold)
            {
                product.StockQuantity -= quantitySold;
            }
            else
            {
                // Có thể ném ra một lỗi để Controller xử lý, ví dụ: hiển thị thông báo cho người dùng
                throw new InvalidOperationException($"Sản phẩm '{product.Name}' không đủ số lượng trong kho.");
            }
            await _context.SaveChangesAsync();
        }
    }
    public async Task UpdateApprovalStatusAsync(int id, ProductApprovalStatus status)
    {
        var product = await _context.Products.FindAsync(id);
        if (product != null)
        {
            product.ApprovalStatus = status;
            _context.Update(product);
            await _context.SaveChangesAsync();
        }
    }
    public void AddProductImage(ProductImage image)
    {
        _context.ProductImages.Add(image);
        _context.SaveChanges();
    }

    // Xóa phương thức GetByUserIdAsync bị trùng lặp ở cuối file
}