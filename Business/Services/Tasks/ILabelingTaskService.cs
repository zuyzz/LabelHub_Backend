using DataLabelProject.Application.DTOs.Tasks;
using DataLabelProject.Business.Models;
using DataLabelProject.Business.Models.Enums;

namespace DataLabelProject.Business.Services.Tasks;

public interface ILabelingTaskService
{
    Task<(List<LabelingTask> Tasks, int TotalCount)> GetTasksForReviewerAsync(Guid reviewerId, LabelingTaskStatus? status, int page, int pageSize);
    Task<(List<LabelingTask> Tasks, int TotalCount)> GetTasksForAnnotatorAsync(Guid annotatorId, LabelingTaskStatus? status, int page, int pageSize);
    Task<LabelingTask?> GetTaskByIdForUserAsync(Guid taskId, Guid userId);
    Task<List<LabelingTaskItem>> AssignTaskItemsToTaskAsync(Guid taskId, IEnumerable<Guid> taskItemIds);
    Task<List<LabelingTaskItem>> GetTaskItemsByProjectIdAsync(Guid projectId);
    Task<List<Assignment>> BulkAssignTasksAsync(Guid datasetId, Guid projectId, Guid assignedTo, Guid assignedBy);
    Task<List<Assignment>> UpdateAssignmentsByDatasetAsync(Guid assignmentId, Guid datasetId, double timeLimitMinutes);
}
