namespace DataLabelProject.Application.DTOs.Tasks;

public class TaskAssignmentInfo
{
    public Guid TaskId { get; set; }
    public Guid ProjectId { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid AssignedTo { get; set; }
    public Guid AssignedBy { get; set; }
    public DateTime AssignedAt { get; set; }
    public DateTime DeadlineAt { get; set; }
}
