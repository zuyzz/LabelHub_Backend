using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DataLabelProject.Application.DTOs.Annotations;

public class AnnotationPayload
{
    public string Type { get; set; } = null!;
    public string Label { get; set; } = null!;
    public double? X { get; set; }
    public double? Y { get; set; }
    public double? Width { get; set; }
    public double? Height { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}
