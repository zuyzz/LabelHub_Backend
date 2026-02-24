namespace DataLabelProject.Application.DTOs.Datasets
{
    /// <summary>
    /// Response model for dataset item.
    /// </summary>
    public record DatasetItemResponse(
        Guid ItemId,
        Guid DatasetId,
        string MediaType,
        string StorageUri,
        DateTime? CreatedAt
    );
}
