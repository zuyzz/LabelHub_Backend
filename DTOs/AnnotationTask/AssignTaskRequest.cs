using System.ComponentModel.DataAnnotations;

namespace DataLabel_Project_BE.DTOs.AnnotationTask;

public class AssignTaskRequest
{
    /// <summary>
    /// Annotator user ID to assign the task to
    /// </summary>
    [Required(ErrorMessage = "Annotator user ID is required")]
    public Guid AnnotatorId { get; set; }
}
