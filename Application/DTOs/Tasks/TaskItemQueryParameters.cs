using DataLabelProject.Application.DTOs.Common;
using DataLabelProject.Business.Models.Enums;

namespace DataLabelProject.Application.DTOs.Tasks;

public class TaskItemQueryParameters : PaginationParameters
{
    public LabelingTaskItemStatus? Status { get; set; }
    public bool? IsExpired { get; set; }
}
