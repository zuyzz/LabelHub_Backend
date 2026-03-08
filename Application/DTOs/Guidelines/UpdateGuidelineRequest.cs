using System.ComponentModel.DataAnnotations;

namespace DataLabelProject.Application.DTOs.Guidelines;

public class UpdateGuidelineRequest
{
    /// <summary>
    /// Guideline content/instructions
    /// </summary>
    [Required(ErrorMessage = "Content is required")]
    public string Content { get; set; } = null!;
}
