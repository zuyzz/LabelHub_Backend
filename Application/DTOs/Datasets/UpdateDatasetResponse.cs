namespace DataLabelProject.Application.DTOs.Datasets
{
    /// <summary>
    /// Response model for updating a dataset.
    /// </summary>
    public record UpdateDatasetResponse(
        Guid DatasetId,
        string Name,
        string? Description,
        Guid? ProjectId
    );
}
