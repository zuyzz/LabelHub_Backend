using System.ComponentModel.DataAnnotations;

namespace DataLabelProject.Application.DTOs.Tasks;

public class UpdateAssignmentDeadlineRequest
{
    [Required]
    public Guid TaskId { get; set; }

    [Required]
    public DateTime DeadlineAt { get; set; }
}
