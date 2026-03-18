namespace DataLabelProject.Application.DTOs.Tasks;

public class TaskAssignmentResponse
{
    public Guid TaskId { get; set; }
    public Guid ProjectId { get; set; }
    public Guid AssignedTo { get; set; }
    public Guid AssignedBy { get; set; }
    public DateTime AssignedAt { get; set; }
    public DateTime DeadlineAt { get; set; }
}
