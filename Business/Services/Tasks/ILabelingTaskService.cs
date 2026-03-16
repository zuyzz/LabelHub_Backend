using DataLabelProject.Application.DTOs.Common;
using DataLabelProject.Application.DTOs.Tasks;
using DataLabelProject.Business.Models;
using DataLabelProject.Business.Models.Enums;

namespace DataLabelProject.Business.Services.Tasks;

public interface ILabelingTaskService
{
    Task<PagedResult<TaskAssignmentInfo>> GetTasksAsync(Guid userId, string userRole, TaskQueryParameters @params);
    Task<LabelingTask?> GetTaskByIdForUserAsync(Guid taskId, Guid userId, string userRole);
    Task<List<LabelingTaskItem>> AssignTaskItemsToTaskAsync(Guid taskId, IEnumerable<Guid> taskItemIds);
    Task<List<LabelingTaskItem>> GetTaskItemsByProjectIdAsync(Guid projectId);
    Task<List<Assignment>> UpdateAssignmentsByDatasetAsync(Guid assignmentId, Guid datasetId, double timeLimitMinutes);
}
