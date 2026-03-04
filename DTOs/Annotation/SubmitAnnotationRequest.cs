using System.ComponentModel.DataAnnotations;

namespace DataLabel_Project_BE.DTOs.Annotation;

public class SubmitAnnotationRequest
{
    [Required]
    public string AnnotationPayload { get; set; } = null!;

    public List<string>? RequiredFields { get; set; }
}
