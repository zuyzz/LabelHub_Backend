using DataLabelProject.Application.DTOs.Common;
using DataLabelProject.Application.DTOs.Tasks;
using DataLabelProject.Business.Models;

namespace DataLabelProject.Business.Services.TaskItems;

public interface ITaskItemService
{
    Task<PagedResponse<TaskItemDetailResponse>> GetTaskItemsByTaskAsync(Guid taskId, TaskItemQueryParameters @params);
}
