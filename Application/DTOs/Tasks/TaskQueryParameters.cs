using DataLabelProject.Application.DTOs.Common;
using DataLabelProject.Business.Models.Enums;

namespace DataLabelProject.Application.DTOs.Tasks;

public class TaskQueryParameters : PaginationParameters
{
    public LabelingTaskStatus? Status { get; set; }
    public bool? IsAvailable { get; set; }
}
