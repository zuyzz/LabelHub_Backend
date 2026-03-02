using DataLabelProject.Business.Models;

namespace DataLabelProject.Data.Repositories.Abstractions;

public interface IProjectDatasetRepository
{
    /// <summary>
    /// Attach a dataset to a project
    /// </summary>
    Task<ProjectDataset> AttachDatasetAsync(ProjectDataset projectDataset);

    /// <summary>
    /// Detach a dataset from a project
    /// </summary>
    Task DetachDatasetAsync(Guid projectId, Guid datasetId);

    /// <summary>
    /// Check if a dataset is already attached to a project
    /// </summary>
    Task<bool> IsDatasetAttachedAsync(Guid projectId, Guid datasetId);

    /// <summary>
    /// Get all datasets attached to a project
    /// </summary>
    Task<IEnumerable<Dataset>> GetDatasetsByProjectAsync(Guid projectId);

    /// <summary>
    /// Get all projects a dataset is attached to
    /// </summary>
    Task<IEnumerable<Project>> GetProjectsByDatasetAsync(Guid datasetId);

    /// <summary>
    /// Save changes to the database
    /// </summary>
    Task SaveChangesAsync();
}
