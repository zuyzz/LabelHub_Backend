using DataLabelProject.Business.Models;
using DataLabelProject.Application.DTOs.Projects;

namespace DataLabelProject.Data.Repositories.Abstractions
{
    public interface IProjectRepository
    {
        IQueryable<Project> Query();
        Task<Project?> GetByIdAsync(Guid id);
        Task<Project?> GetByNameAndCreatorAsync(string name, Guid userId);
        Task CreateAsync(Project project);
        Task UpdateAsync(Project project);
        Task DeleteAsync(Project project);
        Task SaveChangesAsync();
    }
}
