using DataLabelProject.Application.DTOs.Datasets;

namespace DataLabelProject.Business.Services.Projects;

public interface IProjectDatasetService
{
    /// <summary>
    /// Attach a dataset to a project
    /// Only allowed if current user owns both the dataset and project (is their creator)
    /// </summary>
    Task<AttachDatasetResponse> AttachDatasetAsync(Guid datasetId, Guid projectId);

    /// <summary>
    /// Detach a dataset from a project
    /// </summary>
    Task DetachDatasetAsync(Guid projectId, Guid datasetId);

    /// <summary>
    /// Check if a dataset is attached to a project
    /// </summary>
    Task<bool> IsDatasetAttachedAsync(Guid projectId, Guid datasetId);
}
