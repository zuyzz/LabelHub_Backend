using DataLabelProject.Business.Models;

namespace DataLabelProject.Business.Services.Projects
{
    public interface IProjectVersionService
    {
        Task<ProjectVersion> GetByIdAsync(Guid projectVersionId);

        Task<ProjectVersion?> GetDraftAsync(Guid projectId);

        Task<ProjectVersion?> GetLatestReleasedAsync(Guid projectId);

        Task<IEnumerable<ProjectVersion>> GetAllByProjectAsync(Guid projectId);

        Task<ProjectVersion> CreateDraftAsync(
            Guid projectId,
            Guid? datasetId,
            Guid? labelSetId,
            Guid? guidelineId
        );

        Task ReleaseAsync(Guid projectVersionId);
    }
}
