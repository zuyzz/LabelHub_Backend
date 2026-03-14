using DataLabelProject.Business.Models;

namespace DataLabelProject.Business.Services.Annotations;

public interface IAnnotationService
{
    Task<IEnumerable<Annotation>> GetAnnotationsForUserAsync(Guid currentUserId, string currentUserRole, string? status);
    Task<Annotation> CreateAnnotationAsync(Guid taskId, string payloadJson, Guid currentUserId, string currentUserRole);
}
