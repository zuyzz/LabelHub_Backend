using System.Text.Json;
using DataLabelProject.Application.DTOs.Annotations;
using DataLabelProject.Business.Models;
using DataLabelProject.Business.Models.Enums;
using DataLabelProject.Data.Repositories.Abstractions;

namespace DataLabelProject.Business.Services.Annotations;

public class AnnotationService : IAnnotationService
{
    private readonly IAnnotationRepository _annotationRepository;
    private readonly ILabelingTaskRepository _taskRepository;

    public AnnotationService(
        IAnnotationRepository annotationRepository,
        ILabelingTaskRepository taskRepository)
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

        return annotations.Where(a => a.Reviews.Any(r => r.Result == parsedStatus));
    }

    public async Task<Annotation> CreateAnnotationAsync(CreateAnnotationRequest request, Guid currentUserId, string currentUserRole)
    {
        if (currentUserRole != "annotator")
            throw new UnauthorizedAccessException("Only annotator can create annotations");

        if (request.Payload == null)
            throw new ArgumentException("Payload is required");

        var payload = request.Payload;

        if (payload.ExtensionData != null && payload.ExtensionData.Count > 0)
            throw new ArgumentException($"Payload contains unexpected field(s): {string.Join(", ", payload.ExtensionData.Keys)}");

        if (string.IsNullOrWhiteSpace(payload.Type) || !payload.Type.Equals("bbox", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Payload type must be 'bbox'");

        if (string.IsNullOrWhiteSpace(payload.Label))
            throw new ArgumentException("Payload label is required");

        if (!payload.X.HasValue)
            throw new ArgumentException("Payload x is required");

        if (!payload.Y.HasValue)
            throw new ArgumentException("Payload y is required");

        if (!payload.Width.HasValue)
            throw new ArgumentException("Payload width is required");

        if (!payload.Height.HasValue)
            throw new ArgumentException("Payload height is required");

        if (payload.Width <= 0 || payload.Height <= 0)
            throw new ArgumentException("Payload width and height must be greater than zero");

        var task = await _taskRepository.GetByIdAsync(request.TaskId);
        if (task == null)
            throw new KeyNotFoundException("Task not found");

        // AnnotatorId is inferred from current user context
        var annotatorId = currentUserId;

        var payloadJson = JsonSerializer.Serialize(payload);

        var annotation = new Annotation
        {
            AnnotationId = Guid.NewGuid(),
            TaskId = request.TaskId,
            AnnotatorId = annotatorId,
            Payload = payloadJson,
            SubmittedAt = DateTime.UtcNow
        };

        await _annotationRepository.AddAsync(annotation);
        await _annotationRepository.SaveChangesAsync();

        return annotation;
    }
}
