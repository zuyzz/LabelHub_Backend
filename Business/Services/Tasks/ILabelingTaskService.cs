using DataLabelProject.Business.Models;

namespace DataLabelProject.Business.Services.Tasks;

public interface ILabelingTaskService
{
    Task<List<LabelingTask>> GetTasksForUserAsync(Guid currentUserId, string currentUserRole);
    Task<LabelingTask> CreateTaskAsync(Guid datasetItemId, Guid projectId);
    Task<Assignment> AssignTaskAsync(Guid taskId, Guid projectId, Guid assignedTo, Guid assignedBy);
    Task<Assignment> UpdateDeadlineAsync(Guid taskId, DateTime deadlineAt);
}
