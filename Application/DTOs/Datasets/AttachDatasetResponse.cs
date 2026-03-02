namespace DataLabelProject.Application.DTOs.Datasets
{
    /// <summary>
    /// Response model for attaching a dataset to a project.
    /// </summary>
    public record AttachDatasetResponse(
        Guid ProjectId,
        Guid DatasetId,
        DateTime AttachedAt,
        Guid? AttachedBy
    );
}
