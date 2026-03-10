using System.ComponentModel.DataAnnotations;

namespace DataLabelProject.Application.DTOs.Tasks;

public class CreateTaskRequest
{
    [Required] public Guid DatasetItemId { get; set; }
    [Required] public Guid ProjectId { get; set; }
}
