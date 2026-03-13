using DataLabelProject.Business.Models;

namespace DataLabelProject.Data.Repositories.Abstractions
{
    public interface IProjectConfigRepository
    {
        Task<ProjectConfig?> GetLatestByProjectIdAsync(Guid projectId);
    }
}