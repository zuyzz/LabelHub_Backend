using DataLabelProject.Application.DTOs.Annotations;
using DataLabelProject.Business.Models;

namespace DataLabelProject.Business.Services.Annotations;

public interface IAnnotationService
{
    Task<IEnumerable<Annotation>> GetAnnotationsForUserAsync(Guid currentUserId, string currentUserRole, string? status);
    Task<Annotation> CreateAnnotationAsync(CreateAnnotationRequest request, Guid currentUserId, string currentUserRole);
}
