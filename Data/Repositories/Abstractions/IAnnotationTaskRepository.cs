using DataLabelProject.Business.Models;

namespace DataLabelProject.Data.Repositories.Abstractions;

public interface IAnnotationTaskRepository
{
    Task<DatasetItem?> GetDatasetItemByIdAsync(Guid datasetItemId);
    Task<Dataset?> GetDatasetByIdAsync(Guid datasetId);
    Task<Project?> GetProjectByIdAsync(Guid projectId);
    Task<SystemConfig?> GetSystemConfigAsync();
    Task<AnnotationTask?> GetTaskWithDatasetItemAsync(Guid taskId);
    Task<AnnotationTask?> GetTaskWithAssignmentsAsync(Guid taskId);
    Task<AnnotationTask?> GetTaskByIdWithRelationsAsync(Guid taskId);
    Task<List<AnnotationTask>> GetTasksForManagerAsync();
    Task<List<AnnotationTask>> GetTasksForAnnotatorAsync(Guid annotatorId);
    Task<bool> HasAssignmentAsync(Guid taskId);
    Task<User?> GetUserByIdAsync(Guid userId);
    Task<Role?> GetRoleByIdAsync(Guid roleId);
    Task<Guid> GetProjectIdByDatasetItemIdAsync(Guid datasetItemId);
    Task<List<Assignment>> GetAssignmentsByTaskIdAsync(Guid taskId);
    void AddTask(AnnotationTask task);
    void AddAssignment(Assignment assignment);
    void AddActivityLog(ActivityLog activityLog);
    void RemoveAssignments(IEnumerable<Assignment> assignments);
    Task SaveChangesAsync();
}
