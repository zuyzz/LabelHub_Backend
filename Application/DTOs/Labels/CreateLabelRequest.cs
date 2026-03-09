using System.ComponentModel.DataAnnotations;

namespace DataLabelProject.Application.DTOs.Labels
{
    public class CreateLabelRequest
    {
        public Guid CategoryId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; } = null!;
    }
}
