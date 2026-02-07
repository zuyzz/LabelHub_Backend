using DataLabel_Project_BE.Models;
using DataLabel_Project_BE.Repositories;

namespace DataLabel_Project_BE.Services
{
    public class ProjectVersionService : IProjectVersionService
    {
        private readonly IProjectVersionRepository _projectVersionRepo;

        public ProjectVersionService(IProjectVersionRepository projectVersionRepo)
        {
            _projectVersionRepo = projectVersionRepo;
        }

        public async Task<ProjectVersion> GetByIdAsync(Guid projectVersionId)
        {
            var pv = await _projectVersionRepo.GetByIdAsync(projectVersionId);
            if (pv == null)
                throw new KeyNotFoundException("Project version not found");

            return pv;
        }

        public Task<ProjectVersion?> GetDraftAsync(Guid projectId)
        {
            return _projectVersionRepo.GetDraftByProjectIdAsync(projectId);
        }

        public Task<ProjectVersion?> GetLatestReleasedAsync(Guid projectId)
        {
            return _projectVersionRepo.GetLatestReleasedByProjectIdAsync(projectId);
        }

        public Task<IEnumerable<ProjectVersion>> GetAllByProjectAsync(Guid projectId)
        {
            return _projectVersionRepo.GetAllByProjectIdAsync(projectId);
        }

        public async Task<ProjectVersion> CreateDraftAsync(
            Guid projectId,
            Guid datasetId,
            Guid labelSetId,
            Guid guidelineId,
            Guid createdBy
        )
        {
            var existingDraft = await _projectVersionRepo.GetDraftByProjectIdAsync(projectId);
            if (existingDraft != null)
                throw new InvalidOperationException("A draft version already exists for this project");

            var nextVersion = await _projectVersionRepo.GetNextVersionNumberAsync(projectId);

            var projectVersion = new ProjectVersion
            {
                ProjectId = projectId,
                DatasetId = datasetId,
                LabelSetId = labelSetId,
                GuidelineId = guidelineId,
                VersionNumber = nextVersion,
                CreatedAt = DateTime.UtcNow,
            };

            await _projectVersionRepo.AddAsync(projectVersion);
            await _projectVersionRepo.SaveChangesAsync();

            return projectVersion;
        }

        public async Task ReleaseAsync(Guid projectVersionId)
        {
            var pv = await GetByIdAsync(projectVersionId);

            if (pv.ReleasedAt != null)
                throw new InvalidOperationException("Project version is already released");

            pv.ReleasedAt = DateTime.UtcNow;

            await _projectVersionRepo.UpdateAsync(pv);
            await _projectVersionRepo.SaveChangesAsync();
        }
    }
}
