using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DataLabelProject.Application.DTOs.Annotations;

public class AnnotationPayload
{
    [Required(ErrorMessage = "Bboxes is required")]
    [MinLength(1, ErrorMessage = "At least one bbox is required")]
    [JsonPropertyName("bboxes")]
    public List<BboxItem> Bboxes { get; set; } = new();
}

public class BboxItem
{
    [Required(ErrorMessage = "Label is required")]
    [JsonPropertyName("label")]
    public string Label { get; set; } = null!;

    [Required(ErrorMessage = "X coordinate is required")]
    [JsonPropertyName("x")]
    public double? X { get; set; }

    [Required(ErrorMessage = "Y coordinate is required")]
    [JsonPropertyName("y")]
    public double? Y { get; set; }

    [Required(ErrorMessage = "Width is required")]
    [Range(0.000001, double.MaxValue, ErrorMessage = "Width must be greater than 0")]
    [JsonPropertyName("width")]
    public double? Width { get; set; }

    [Required(ErrorMessage = "Height is required")]
    [Range(0.000001, double.MaxValue, ErrorMessage = "Height must be greater than 0")]
    [JsonPropertyName("height")]
    public double? Height { get; set; }
}
