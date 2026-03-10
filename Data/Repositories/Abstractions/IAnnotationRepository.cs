using DataLabelProject.Business.Models;

namespace DataLabelProject.Data.Repositories.Abstractions;

public interface IAnnotationRepository
{
    Task<IEnumerable<Annotation>> GetAllAsync();
    Task<IEnumerable<Annotation>> GetByAnnotatorIdAsync(Guid annotatorId);
    Task<IEnumerable<Annotation>> GetApprovedByTaskIdAsync(Guid taskId);
    Task<Annotation?> GetByIdAsync(Guid annotationId);
    Task AddAsync(Annotation annotation);
    Task SaveChangesAsync();
}
