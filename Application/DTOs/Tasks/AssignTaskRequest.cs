using System.ComponentModel.DataAnnotations;

namespace DataLabelProject.Application.DTOs.Tasks;

public class AssignTaskRequest
{
    [Required] public Guid AssignedTo { get; set; }
    [Required] public Guid ProjectId { get; set; }
    [Required] public Guid TaskId { get; set; }
}
