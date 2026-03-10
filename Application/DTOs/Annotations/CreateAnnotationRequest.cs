using System.ComponentModel.DataAnnotations;

namespace DataLabelProject.Application.DTOs.Annotations;

public class CreateAnnotationRequest
{
    [Required]
    public Guid TaskId { get; set; }

    [Required]
    public AnnotationPayload Payload { get; set; } = null!;
}
