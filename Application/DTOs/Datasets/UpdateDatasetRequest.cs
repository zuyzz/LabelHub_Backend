namespace DataLabelProject.Application.DTOs.Datasets
{
    /// <summary>
    /// Request model for updating a dataset.
    /// </summary>
    public class UpdateDatasetRequest
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
    }
}
