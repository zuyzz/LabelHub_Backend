namespace DataLabelProject.Application.DTOs.Tasks;

public class TaskItemResponse
{
    public Guid TaskItemId { get; set; }
    public Guid DatasetItemId { get; set; }
    public Guid? TaskId { get; set; }
    public int RevisionCount { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class TaskResponse
{
    public Guid TaskId { get; set; }
    public Guid ProjectId { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<TaskItemResponse> TaskItems { get; set; } = new();
}
