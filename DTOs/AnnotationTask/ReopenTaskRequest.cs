namespace DataLabel_Project_BE.DTOs.AnnotationTask;

/// <summary>
/// Request DTO for reopening a task
/// </summary>
public class ReopenTaskRequest
{
    /// <summary>
    /// Optional reason for reopening the task
    /// </summary>
    public string? Reason { get; set; }
}
