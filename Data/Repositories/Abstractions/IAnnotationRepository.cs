using DataLabelProject.Business.Models;

namespace DataLabelProject.Data.Repositories.Abstractions;

public interface IAnnotationRepository
{
    Task<IEnumerable<Annotation>> GetAllAsync();
    Task<IEnumerable<Annotation>> GetByAnnotatorIdAsync(Guid annotatorId);
    Task<IEnumerable<Annotation>> GetByTaskItemIdAsync(Guid taskItemId);
    Task<Annotation?> GetByIdAsync(Guid annotationId);
    Task<Annotation?> GetByTaskItemIdAndAnnotatorIdAsync(Guid taskItemId, Guid annotatorId);
    Task AddAsync(Annotation annotation);
    Task SaveChangesAsync();
}
