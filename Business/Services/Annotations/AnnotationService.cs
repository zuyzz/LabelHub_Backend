using System.Linq;
using System.Text.Json;
using DataLabelProject.Application.DTOs.Annotations;
using DataLabelProject.Business.Models;
using DataLabelProject.Business.Models.Enums;
using DataLabelProject.Data.Repositories.Abstractions;

namespace DataLabelProject.Business.Services.Annotations;

public class AnnotationService : IAnnotationService
{
    private readonly IAnnotationRepository _annotationRepository;
    private readonly ILabelingTaskItemRepository _taskRepository;

    public AnnotationService(
        IAnnotationRepository annotationRepository,
        ILabelingTaskItemRepository taskRepository)
    {
        _annotationRepository = annotationRepository;
        _taskRepository = taskRepository;
    }

    public async Task<IEnumerable<Annotation>> GetAnnotationsForUserAsync(Guid currentUserId, string currentUserRole, string? status)
    {
        IEnumerable<Annotation> annotations;

        if (currentUserRole == "admin" || currentUserRole == "manager")
        {
            annotations = await _annotationRepository.GetAllAsync();
        }
        else if (currentUserRole == "annotator")
        {
            annotations = await _annotationRepository.GetByAnnotatorIdAsync(currentUserId);
        }
        else
        {
            throw new UnauthorizedAccessException("Role not authorized to view annotations");
        }

        if (string.IsNullOrEmpty(status))
            return annotations;

        if (!Enum.TryParse<ReviewResult>(status, true, out var parsedStatus))
            throw new ArgumentException("Invalid status filter. Allowed values: approved, rejected.");

        return annotations;
    }

    public async Task<Annotation> CreateAnnotationAsync(Guid taskId, string payloadJson, Guid currentUserId, string currentUserRole)
    {
        if (currentUserRole != "annotator")
            throw new UnauthorizedAccessException("Only annotator can create annotations");

        var task = await _taskRepository.GetByIdAsync(taskId);
        if (task == null)
            throw new KeyNotFoundException("Task not found");

        var annotatorId = currentUserId;

        var annotation = new Annotation
        {
            AnnotationId = Guid.NewGuid(),
            TaskItemId = taskId,
            AnnotatorId = annotatorId,
            Payload = payloadJson,
            SubmittedAt = DateTime.UtcNow
        };

        await _annotationRepository.AddAsync(annotation);
        await _annotationRepository.SaveChangesAsync();

        return annotation;
    }
}
