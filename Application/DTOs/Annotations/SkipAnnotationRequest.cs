using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DataLabelProject.Application.DTOs.Annotations;

public class SkipAnnotationRequest
{
    [Required]
    [JsonPropertyName("taskItemId")]
    public Guid TaskItemId { get; set; }

    [JsonPropertyName("note")]
    public string? Note { get; set; }
}
