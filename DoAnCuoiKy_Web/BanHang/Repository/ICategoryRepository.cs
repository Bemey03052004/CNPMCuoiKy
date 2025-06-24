// ICategoryRepository.cs
using BanHang.Models; // Nếu Category.cs có namespace

public interface ICategoryRepository
{
    Task<IEnumerable<Category>> GetAllAsync();
    Task<Category?> GetByIdAsync(int id); // Cho phép null nếu không tìm thấy
    Task AddAsync(Category category);
    Task UpdateAsync(Category category);
    Task DeleteAsync(int id);
}