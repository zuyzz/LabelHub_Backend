using System.ComponentModel.DataAnnotations;

namespace DataLabelProject.Application.DTOs.Tasks;

public class UpdateAssignmentTimeLimitRequest
{
    [Required] public Guid TaskId { get; set; }
    [Required] public double TimeLimitMinutes { get; set; }
}
