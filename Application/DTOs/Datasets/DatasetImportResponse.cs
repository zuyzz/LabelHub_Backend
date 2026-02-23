namespace DataLabelProject.Application.DTOs.Datasets
{
    public record DatasetImportResponse(
        Guid DatasetId,
        string Name,
        string? Description,
        string? StorageUri,
        int ItemCount
    );
}
