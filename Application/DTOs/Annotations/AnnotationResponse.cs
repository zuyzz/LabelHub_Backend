namespace DataLabelProject.Application.DTOs.Annotations;

public class AnnotationResponse
{
    public Guid AnnotationId { get; set; }
    public Guid TaskId { get; set; }
    public Guid AnnotatorId { get; set; }
    public object Payload { get; set; } = null!;
    public DateTime? SubmittedAt { get; set; }
    public string? Status { get; set; }
}
