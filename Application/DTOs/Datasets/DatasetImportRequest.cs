using Microsoft.AspNetCore.Http;

namespace DataLabelProject.Application.DTOs.Datasets
{
    /// <summary>
    /// Model bound from multipart/form-data in the import endpoint.
    /// </summary>
    public class DatasetImportRequest
    {
        public IFormFile? File { get; set; }

        /// <summary>
        /// Optional dataset display name (used as dataset.Name). If not provided, a default is generated.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Optional dataset description stored on the dataset record.
        /// </summary>
        public string? Description { get; set; }
    }
}
