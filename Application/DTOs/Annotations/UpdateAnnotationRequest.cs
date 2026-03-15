using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DataLabelProject.Application.DTOs.Annotations;

public class UpdateAnnotationRequest
{
    [Required(ErrorMessage = "Payload is required")]
    [JsonPropertyName("payload")]
    public AnnotationPayload Payload { get; set; } = null!;
}
