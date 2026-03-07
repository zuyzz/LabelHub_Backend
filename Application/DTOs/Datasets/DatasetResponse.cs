namespace DataLabelProject.Application.DTOs.Datasets
{
    /// <summary>
    /// Response model for getting a dataset by ID.
    /// </summary>
    public record DatasetResponse(
        Guid DatasetId,
        string Name,
        string? Description,
        DateTime CreatedAt,
        Guid? CreatedBy,
        string MediaType,
        int ItemCount
    );
}
