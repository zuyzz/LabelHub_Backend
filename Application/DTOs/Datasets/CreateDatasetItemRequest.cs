namespace DataLabelProject.Application.DTOs.Datasets
{
    /// <summary>
    /// Request model for creating a new dataset item.
    /// </summary>
    public class CreateDatasetItemRequest
    {
        /// <summary>
        /// The ID of the dataset this item belongs to.
        /// </summary>
        public Guid DatasetId { get; set; }

        /// <summary>
        /// The media type of the item (e.g., "image/png", "image/jpeg").
        /// </summary>
        public string MediaType { get; set; } = null!;

        /// <summary>
        /// The storage URI of the item in file storage.
        /// </summary>
        public string StorageUri { get; set; } = null!;
    }
}
