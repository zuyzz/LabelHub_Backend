using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DataLabelProject.Application.DTOs.Annotations;

public class CreateAnnotationRequest
{
    [Required]
    [JsonPropertyName("taskId")]
    public Guid TaskId { get; set; }

    [Required]
    [JsonPropertyName("payload")]
    public AnnotationPayload Payload { get; set; } = null!;
}
