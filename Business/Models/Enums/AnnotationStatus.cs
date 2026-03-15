using System.Text.Json.Serialization;

namespace DataLabelProject.Business.Models.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AnnotationStatus
{
    Submitted,
    Skipped,
    Conflicted,
    Resolved
}