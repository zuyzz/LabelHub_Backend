using System.ComponentModel.DataAnnotations;

namespace DataLabel_Project_BE.DTOs.AnnotationTask;

public class CreateTaskRequest
{
    /// <summary>
    /// Dataset Item ID that this task belongs to
    /// </summary>
    [Required(ErrorMessage = "Dataset Item ID is required")]
    public Guid DatasetItemId { get; set; }

    /// <summary>
    /// Task scope URI (e.g., data item identifier)
    /// </summary>
    [Required(ErrorMessage = "Scope URI is required")]
    [MaxLength(500, ErrorMessage = "Scope URI cannot exceed 500 characters")]
    public string ScopeUri { get; set; } = null!;

    /// <summary>
    /// Task deadline (optional, uses system default if not provided)
    /// </summary>
    public DateTime? DeadlineAt { get; set; }

    /// <summary>
    /// Consensus type (optional)
    /// </summary>
    [MaxLength(50)]
    public string? Consensus { get; set; }
}
