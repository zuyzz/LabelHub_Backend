using System.ComponentModel.DataAnnotations;

namespace DataLabelProject.Application.DTOs.Guidelines
{
    public class CreateGuidelineRequest
    {
        [Required(ErrorMessage = "Content is required")]
        public string Content { get; set; } = null!;

        public Guid ProjectId { get; set; }
    }
}
