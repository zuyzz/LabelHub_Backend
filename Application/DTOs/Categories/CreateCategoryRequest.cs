using System.ComponentModel.DataAnnotations;

namespace DataLabelProject.Application.DTOs.Categories
{
    public class CreateCategoryRequest
    {
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
    }
}
