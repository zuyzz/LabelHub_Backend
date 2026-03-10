using System.ComponentModel.DataAnnotations;

namespace DataLabelProject.Application.DTOs.Datasets
{
    public class CreateDatasetRequest
    {
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; } = null!;

        public string? Description { get; set; }
    }
}
