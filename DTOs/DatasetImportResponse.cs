namespace DataLabel_Project_BE.DTOs
{
    public record DatasetImportResponse(
        Guid DatasetId,
        string Name,
        string? Description,
        string? StorageUri,
        int ItemCount
    );
}