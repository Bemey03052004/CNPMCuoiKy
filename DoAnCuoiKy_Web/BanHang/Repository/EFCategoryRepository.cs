// EFCategoryRepository.cs
using BanHang.Models; // Nếu Category.cs có namespace
using Microsoft.EntityFrameworkCore;

public class EFCategoryRepository : ICategoryRepository
{
    private readonly ApplicationDbContext _context;

    public EFCategoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        return await _context.Categories.OrderBy(c => c.Name).ToListAsync();
    }

    public async Task<Category?> GetByIdAsync(int id) // Trả về Category?
    {
        return await _context.Categories.FindAsync(id);
    }

    public async Task AddAsync(Category category)
    {
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Category category)
    {
        _context.Categories.Update(category);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category != null)
        {
            if (await _context.Products.AnyAsync(p => p.CategoryId == id))
            {
                throw new InvalidOperationException($"Không thể xóa danh mục '{category.Name}' vì vẫn còn sản phẩm thuộc danh mục này.");
            }
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
        }
    }
}