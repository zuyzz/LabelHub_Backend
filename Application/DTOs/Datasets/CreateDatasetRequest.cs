using Microsoft.AspNetCore.Http;

namespace DataLabelProject.Application.DTOs.Datasets
{
    /// <summary>
    /// Request model for creating a new dataset.
    /// </summary>
    public class CreateDatasetRequest
    {
        /// <summary>
        /// Optional file. Can be a single image or archive containing images.
        /// </summary>
        public IFormFile? File { get; set; }

        /// <summary>
        /// Optional dataset name. If not provided, a default name will be generated from the file name.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Optional dataset description.
        /// </summary>
        public string? Description { get; set; }
    }
}
