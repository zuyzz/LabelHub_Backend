using DataLabelProject.Business.Models;

namespace DataLabelProject.Data.Repositories.Abstractions
{
    public interface IProjectVersionRepository
    {
        Task<ProjectVersion?> GetByIdAsync(Guid projectVersionId);

        Task<ProjectVersion?> GetDraftByProjectIdAsync(Guid projectId);

        Task<ProjectVersion?> GetLatestReleasedByProjectIdAsync(Guid projectId);

        Task<int> GetNextVersionNumberAsync(Guid projectId);

        Task<IEnumerable<ProjectVersion>> GetAllByProjectIdAsync(Guid projectId);

        Task AddAsync(ProjectVersion projectVersion);

        Task UpdateAsync(ProjectVersion projectVersion);

        Task SaveChangesAsync();
    }
}
