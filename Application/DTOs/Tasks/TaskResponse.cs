namespace DataLabelProject.Application.DTOs.Tasks;

public class TaskResponse
{
    public Guid TaskId { get; set; }
    public Guid DatasetItemId { get; set; }
    public Guid ProjectId { get; set; }
    public List<AssignmentResponse> Assignments { get; set; } = new();
}
