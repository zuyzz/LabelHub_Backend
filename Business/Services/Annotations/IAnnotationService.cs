using DataLabel_Project_BE.DTOs.Annotation;

namespace DataLabelProject.Business.Services.Annotations;

public interface IAnnotationService
{
    Task<(AnnotationResponse? Annotation, string? ErrorMessage)> SaveDraftAsync(Guid taskId, Guid annotatorId, SaveDraftRequest request);
    Task<(AnnotationResponse? Annotation, string? ErrorMessage)> SubmitAsync(Guid taskId, Guid annotatorId, SubmitAnnotationRequest request);
}
