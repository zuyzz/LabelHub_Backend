using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DataLabelProject.Application.DTOs.Annotations;

public class AnnotationPayload
{
    [JsonPropertyName("bboxes")]
    public List<BboxItem> Bboxes { get; set; } = new();

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}

public class BboxItem
{
    [JsonPropertyName("label")]
    public string Label { get; set; } = null!;

    [JsonPropertyName("x")]
    public double? X { get; set; }

    [JsonPropertyName("y")]
    public double? Y { get; set; }

    [JsonPropertyName("width")]
    public double? Width { get; set; }

    [JsonPropertyName("height")]
    public double? Height { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}
