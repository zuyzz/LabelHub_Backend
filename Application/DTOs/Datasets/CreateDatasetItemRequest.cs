namespace DataLabelProject.Application.DTOs.Datasets
{
    /// <summary>
    /// Request model for creating a new dataset item.
    /// </summary>
    public class CreateDatasetItemRequest
    {
        public IFormFile File { get; set; } = null!;
    }
}
