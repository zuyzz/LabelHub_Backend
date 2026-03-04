using DataLabelProject.Business.Models;

namespace DataLabelProject.Data.Repositories.Abstractions;

public interface IAnnotationWorkflowRepository
{
    Task<AnnotationTask?> GetTaskWithDatasetContextAsync(Guid taskId);
    Task<Annotation?> GetAnnotationByTaskAndAnnotatorAsync(Guid taskId, Guid annotatorId);
    Task<Annotation?> GetAnnotationWithContextAsync(Guid annotationId);
    Task<User?> GetUserByIdAsync(Guid userId);
    Task<Role?> GetRoleByIdAsync(Guid roleId);
    Task<SystemConfig?> GetSystemConfigAsync();
    Task<int> CountRejectedReviewsAsync(Guid annotationId);
    Task<Guid> GetProjectIdByTaskIdAsync(Guid taskId);
    Task<Guid> GetProjectIdByAnnotationIdAsync(Guid annotationId);
    void AddAnnotation(Annotation annotation);
    void AddReview(Review review);
    void AddActivityLog(ActivityLog log);
    Task SaveChangesAsync();
}
