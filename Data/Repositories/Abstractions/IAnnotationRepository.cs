using DataLabelProject.Business.Models;

namespace DataLabelProject.Data.Repositories.Abstractions;

public interface IAnnotationRepository
{
    Task<IEnumerable<Annotation>> GetAllAsync();
    Task<IEnumerable<Annotation>> GetByAnnotatorIdAsync(Guid annotatorId);
    Task<IEnumerable<Annotation>> GetApprovedByTaskItemIdAsync(Guid taskItemId);
    Task<Annotation?> GetByIdAsync(Guid annotationId);
    Task<Annotation?> GetByTaskIdAndAnnotatorIdAsync(Guid taskId, Guid annotatorId);
    Task AddAsync(Annotation annotation);
    Task SaveChangesAsync();
}
