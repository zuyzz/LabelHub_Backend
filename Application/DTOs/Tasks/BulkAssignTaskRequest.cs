using System.ComponentModel.DataAnnotations;

namespace DataLabelProject.Application.DTOs.Tasks;
public class BulkAssignTaskRequest
{
    [Required]
    public Guid ProjectId { get; set; }

    [Required]
    public Guid DatasetId { get; set; }

    [Required]
    public List<AssignTaskRequest> Assigments { get; set; } = new ();
}
public class AssignTaskRequest
{
    [Required]
    public Guid AssignedTo { get; set; }

    [Required]
    public DateTime StartedAt { get; set; }

    [Required]
    public DateTime DeadlineAt { get; set; }
}
