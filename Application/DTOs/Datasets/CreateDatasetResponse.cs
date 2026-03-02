namespace DataLabelProject.Application.DTOs.Datasets
{
    /// <summary>
    /// Response model for creating a dataset.
    /// </summary>
    public record CreateDatasetResponse(
        Guid DatasetId,
        string Name,
        string? Description,
        int ItemCount
    );
}
