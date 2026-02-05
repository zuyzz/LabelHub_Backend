using DataLabel_Project_BE.Models;

namespace DataLabel_Project_BE.Repositories
{
    public interface ICategoryRepository
    {
        Task<List<Category>> GetAllAsync();
        Task<Category?> GetByIdAsync(Guid id);
        Task<Category> CreateAsync(Category category);
        Task UpdateAsync(Category category);
    }
}
