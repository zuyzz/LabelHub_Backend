using System.ComponentModel.DataAnnotations;

namespace DataLabelProject.Application.DTOs.Tasks;

public class AssignTaskItemsRequest
{
    [Required]
    public List<Guid> TaskItemIds { get; set; } = new();
}
