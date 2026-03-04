using DataLabel_Project_BE.DTOs.AnnotationTask;

namespace DataLabelProject.Business.Services.AnnotationTasks;

public interface IAnnotationTaskService
{
    Task<(TaskResponse? Task, string? ErrorMessage)> CreateTask(CreateTaskRequest request, Guid managerId);
    Task<(AssignmentInfo? Assignment, string? ErrorMessage)> AssignTask(Guid taskId, Guid annotatorId, Guid managerId);
    Task<List<TaskResponse>> GetTasksForManager(Guid managerId);
    Task<List<TaskResponse>> GetTasksForAnnotator(Guid annotatorId);
    Task<TaskResponse?> GetTaskById(Guid taskId);
    Task<(TaskResponse? Task, string? ErrorMessage)> UpdateTaskStatus(Guid taskId, string newStatus, Guid userId, string userRole);
    Task<(TaskResponse? Task, string? ErrorMessage)> ReopenTask(Guid taskId, Guid managerId, string? reason);
    Task<(bool success, string? errorMessage)> DeleteTask(Guid taskId, Guid managerId);
}
