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

        if (payload.Bboxes == null || !payload.Bboxes.Any())
            throw new ArgumentException("Payload bboxes is required and must contain at least one bbox");

        if (payload.ExtensionData != null && payload.ExtensionData.Count > 0)
            throw new ArgumentException($"Payload contains unexpected field(s): {string.Join(", ", payload.ExtensionData.Keys)}");

        for (var i = 0; i < payload.Bboxes.Count; i++)
        {
            var box = payload.Bboxes[i];
            if (box == null)
                throw new ArgumentException($"Payload bboxes[{i}] is null");

            if (string.IsNullOrWhiteSpace(box.Label))
                throw new ArgumentException($"Payload bboxes[{i}].label is required");

            if (!box.X.HasValue)
                throw new ArgumentException($"Payload bboxes[{i}].x is required");

            if (!box.Y.HasValue)
                throw new ArgumentException($"Payload bboxes[{i}].y is required");

            if (!box.Width.HasValue)
                throw new ArgumentException($"Payload bboxes[{i}].width is required");

            if (!box.Height.HasValue)
                throw new ArgumentException($"Payload bboxes[{i}].height is required");

            if (box.Width <= 0 || box.Height <= 0)
                throw new ArgumentException($"Payload bboxes[{i}].width and height must be greater than zero");

            if (box.ExtensionData != null && box.ExtensionData.Count > 0)
                throw new ArgumentException($"Payload bboxes[{i}] contains unexpected field(s): {string.Join(", ", box.ExtensionData.Keys)}");
        }

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
