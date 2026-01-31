using DataLabel_Project_BE.Models;

namespace DataLabel_Project_BE.Repositories
{
    public interface IProjectRepository
    {
        Task<IEnumerable<Project>> GetAllAsync();
        Task<Project?> GetByIdAsync(Guid id);
        Task AddAsync(Project project);
        Task UpdateAsync(Project project);
        Task DeleteAsync(Project project);
        Task<bool> ExistsAsync(Guid id);
        Task SaveChangesAsync();
    }
}