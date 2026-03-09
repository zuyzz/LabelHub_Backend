namespace DataLabelProject.Application.DTOs.Datasets
{
    public record DatasetItemResponse
    {
        public Guid ItemId { get; set; }
        public Guid DatasetId { get; set; }
        public string MediaType { get; set; } = null!;
        public string StorageUri { get; set; } = null!;
        public string Metadata { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    };
}
