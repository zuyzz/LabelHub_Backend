using System.Text.Json.Serialization;

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
        [JsonPropertyName("width")]
        public int Width { get; set; }

        [JsonPropertyName("height")]
        public int Height { get; set; }

        [JsonPropertyName("fileSize")]
        public long FileSize { get; set; }
    }
}
