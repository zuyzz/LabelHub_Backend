using DataLabelProject.Application.DTOs.Annotations;
using DataLabelProject.Application.DTOs.Common;
using DataLabelProject.Business.Models;

namespace DataLabelProject.Business.Services.Annotations;

public interface IAnnotationService
{
    Task<PagedResponse<AnnotationResponse>> GetAnnotationsByTaskItemAsync(Guid itemId, AnnotationQueryParameters parameters, Guid currentUserId, string currentUserRole);
    Task<PagedResponse<TaskItemAnnotationsResponse>> GetAnnotationsByTaskAsync(Guid taskId, AnnotationQueryParameters parameters, Guid currentUserId, string currentUserRole);
    Task<AnnotationResponse> SubmitAnnotationAsync(SubmitAnnotationRequest request, Guid currentUserId);
    Task<AnnotationResponse> UpdateAnnotationAsync(Guid annotationId, UpdateAnnotationRequest request, Guid currentUserId);
    Task<AnnotationResponse> SkipAnnotationAsync(SkipAnnotationRequest request, Guid currentUserId);
}
