namespace DataLabelProject.Application.DTOs.Datasets
{
    public record DatasetItemResponse
    {
        public Guid ItemId { get; set; }
        public Guid DatasetId { get; set; }
        public string MediaType { get; set; } = null!;
        public string StorageUri { get; set; } = null!;
        public ImageMetadata? Metadata { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    };

    public class ImageMetadata
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public long SizeBytes { get; set; }
    }
}
