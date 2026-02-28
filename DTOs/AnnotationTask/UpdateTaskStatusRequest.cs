using System.ComponentModel.DataAnnotations;

namespace DataLabel_Project_BE.DTOs.AnnotationTask;

public class UpdateTaskStatusRequest
{
    /// <summary>
    /// New status: pending, in_progress, completed, rejected
    /// </summary>
    [Required(ErrorMessage = "Status is required")]
    [RegularExpression("^(pending|in_progress|completed|rejected)$", 
        ErrorMessage = "Status must be: pending, in_progress, completed, or rejected")]
    public string Status { get; set; } = null!;
}
