using DataLabelProject.Business.Models;
using DataLabelProject.Application.DTOs.Projects;

namespace DataLabelProject.Data.Repositories.Abstractions
{
    public interface IProjectRepository
    {
        Task<(IEnumerable<Project> Items, int TotalCount)> GetAllAsync(ProjectQueryParameters @params);
        Task<(IEnumerable<Project> Items, int TotalCount)> GetAllByUserAsync(Guid userId, ProjectQueryParameters @params);
        Task<Project?> GetByIdAsync(Guid id);
        Task<Project?> GetByNameAndCreatorAsync(string name, Guid userId);
        Task CreateAsync(Project project);
        Task UpdateAsync(Project project);
        Task DeleteAsync(Project project);
        Task<bool> ExistsAsync(Guid id);
        Task SaveChangesAsync();
    }
}
