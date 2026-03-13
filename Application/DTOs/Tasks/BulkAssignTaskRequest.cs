using System.ComponentModel.DataAnnotations;

namespace DataLabelProject.Application.DTOs.Tasks;

public class BulkAssignTaskRequest
{
    [Required]
    public Guid AssignedTo { get; set; }

    [Required]
    public Guid ProjectId { get; set; }

    [Required]
    public Guid DatasetId { get; set; }
}
