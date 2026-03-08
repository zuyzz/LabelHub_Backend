using System.ComponentModel.DataAnnotations;

namespace DataLabelProject.Application.DTOs.Guidelines;

public class CreateGuidelineRequest
{
    /// <summary>
    /// Guideline content/instructions
    /// </summary>
    [Required(ErrorMessage = "Content is required")]
    public string Content { get; set; } = null!;
}
