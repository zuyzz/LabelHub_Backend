using System.ComponentModel.DataAnnotations;

namespace DataLabel_Project_BE.DTOs.Annotation;

public class SaveDraftRequest
{
    [Required]
    public string AnnotationPayload { get; set; } = null!;
}
