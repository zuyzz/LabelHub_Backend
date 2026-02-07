using DataLabel_Project_BE.Models;

namespace DataLabel_Project_BE.Services
{
    public interface IProjectVersionService
    {
        Task<ProjectVersion> GetByIdAsync(Guid projectVersionId);

        Task<ProjectVersion?> GetDraftAsync(Guid projectId);

        Task<ProjectVersion?> GetLatestReleasedAsync(Guid projectId);

        Task<IEnumerable<ProjectVersion>> GetAllByProjectAsync(Guid projectId);

        Task<ProjectVersion> CreateDraftAsync(
            Guid projectId,
            Guid datasetId,
            Guid labelSetId,
            Guid guidelineId,
            Guid createdBy
        );

        Task ReleaseAsync(Guid projectVersionId);
    }
}
