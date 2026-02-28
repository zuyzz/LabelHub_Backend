namespace DataLabel_Project_BE.DTOs.AnnotationTask;

public class TaskResponse
{
    public Guid TaskId { get; set; }
    public Guid DatasetItemId { get; set; }
    public string DatasetItemName { get; set; } = null!;
    public string ScopeUri { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string? Consensus { get; set; }
    public DateTime? DeadlineAt { get; set; }
    public DateTime? AssignedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<AssignmentInfo>? Assignments { get; set; }
}

public class AssignmentInfo
{
    public Guid AssignmentId { get; set; }
    public Guid UserId { get; set; }
    public string Username { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public Guid AssignedBy { get; set; }
    public string AssignedByName { get; set; } = null!;
    public DateTime AssignedAt { get; set; }
}
