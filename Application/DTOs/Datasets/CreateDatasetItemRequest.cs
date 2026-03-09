using System.ComponentModel.DataAnnotations;

namespace DataLabelProject.Application.DTOs.Datasets
{
    public class CreateDatasetItemRequest
    {
        [Required(ErrorMessage = "File is required")]
        public IFormFile File { get; set; } = null!;
    }
}
