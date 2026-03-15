using System.Text.Json.Serialization;

namespace DataLabelProject.Business.Models.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum LabelingTaskItemStatus
{
    Unassigned,
    Assigned,
    Incompleted,
    Completed,
    Locked
}