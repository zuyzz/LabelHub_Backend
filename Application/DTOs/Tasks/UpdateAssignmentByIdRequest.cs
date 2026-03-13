using System.ComponentModel.DataAnnotations;

namespace DataLabelProject.Application.DTOs.Tasks;

public class UpdateAssignmentByIdRequest
{
    [Required]
    public Guid DatasetId { get; set; }

    [Required]
    [Range(1, double.MaxValue, ErrorMessage = "Time limit must be greater than 0")]
    public double TimeLimitMinutes { get; set; }
}
