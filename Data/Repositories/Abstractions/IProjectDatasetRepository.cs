using DataLabelProject.Business.Models;

namespace DataLabelProject.Data.Repositories.Abstractions;

public interface IProjectDatasetRepository
{
    Task<ProjectDataset?> GetByIdAsync(Guid projectId, Guid datasetId);
    Task<ProjectDataset> CreateAsync(ProjectDataset projectDataset);
    Task DeleteAsync(ProjectDataset projectDataset);
    Task SaveChangesAsync();
}
