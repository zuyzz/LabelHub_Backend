namespace DataLabelProject.Application.DTOs.Tasks;

public class AssignmentResponse
{
    public Guid AssignmentId { get; set; }
    public Guid TaskId { get; set; }
    public Guid AssignedTo { get; set; }
    public Guid AssignedBy { get; set; }
    public DateTime AssignedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public double TimeLimitMinutes { get; set; }
    public string Status { get; set; } = null!;
}
