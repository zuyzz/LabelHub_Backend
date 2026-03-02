using System.ComponentModel.DataAnnotations;

namespace DataLabelProject.Application.DTOs.Datasets
{
    /// <summary>
    /// Request model for creating a new dataset.
    /// </summary>
    public class CreateDatasetRequest
    {
        /// <summary>
        /// Dataset name (required).
        /// </summary>
        [Required]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Optional dataset description.
        /// </summary>
        public string? Description { get; set; }
    }
}
