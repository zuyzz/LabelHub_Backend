using DataLabel_Project_BE.Models;

namespace DataLabel_Project_BE.Repositories
{
    public interface IProjectRepository
    {
        Task<IEnumerable<Project>> GetAllAsync();
        Task<(IEnumerable<Project> Items, int TotalCount)> GetFilteredAsync(DTOs.ProjectQueryParameters query);
        Task<(IEnumerable<Project> Items, int TotalCount)> GetUserProjectsAsync(DTOs.ProjectQueryParameters query, Guid userId);
        Task<Project?> GetByIdAsync(Guid id);
        Task AddAsync(Project project);
        Task UpdateAsync(Project project);
        Task DeleteAsync(Project project);
        Task<bool> ExistsAsync(Guid id);

        // Project member helpers
        Task<bool> ProjectMemberExistsAsync(Guid projectId, Guid userId);
        Task<bool> AddProjectMemberAsync(Guid projectId, Guid userId);

        Task SaveChangesAsync();
    }
}