using System.ComponentModel.DataAnnotations;

namespace DataLabel_Project_BE.DTOs.Guideline;

public class CreateGuidelineRequest
{
    /// <summary>
    /// Guideline title
    /// </summary>
    [Required(ErrorMessage = "Title is required")]
    [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    public string Title { get; set; } = null!;

    /// <summary>
    /// Guideline content/instructions
    /// </summary>
    [Required(ErrorMessage = "Content is required")]
    public string Content { get; set; } = null!;
}
