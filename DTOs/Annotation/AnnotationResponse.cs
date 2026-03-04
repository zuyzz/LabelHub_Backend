namespace DataLabel_Project_BE.DTOs.Annotation;

public class AnnotationResponse
{
    public Guid AnnotationId { get; set; }
    public Guid TaskId { get; set; }
    public Guid AnnotatorId { get; set; }
    public Guid LabelSetId { get; set; }
    public int LabelSetVersionNumber { get; set; }
    public string AnnotationPayload { get; set; } = null!;
    public bool IsDraft { get; set; }
    public DateTime? SubmittedAt { get; set; }
}
