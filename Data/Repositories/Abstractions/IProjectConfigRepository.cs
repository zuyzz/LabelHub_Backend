using DataLabelProject.Business.Models;

namespace DataLabelProject.Data.Repositories.Abstractions
{
    public interface IProjectConfigRepository
    {
        Task<ProjectConfig?> GetByProjectIdAsync(Guid projectId);
        Task CreateAsync(ProjectConfig config);
        Task DeleteAsync(ProjectConfig config);
        Task SaveChangesAsync();
    }
}