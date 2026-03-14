using System.Linq;
using DataLabelProject.Business.Models;
using DataLabelProject.Business.Models.Enums;
using DataLabelProject.Data.Repositories.Abstractions;

namespace DataLabelProject.Business.Services.Annotations;

public class AnnotationService : IAnnotationService
{
    private readonly IAnnotationRepository _annotationRepository;
    private readonly ILabelingTaskRepository _taskRepository;
    private readonly IAssignmentRepository _assignmentRepository;

    public AnnotationService(
        IAnnotationRepository annotationRepository,
        ILabelingTaskRepository taskRepository,
        IAssignmentRepository assignmentRepository)
    {
        _annotationRepository = annotationRepository;
        _taskRepository = taskRepository;
        _assignmentRepository = assignmentRepository;
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

    public async Task<Annotation> CreateAnnotationAsync(Guid taskId, string payloadJson, Guid currentUserId, string currentUserRole)
    {
        if (currentUserRole != "annotator")
            throw new UnauthorizedAccessException("Only annotator can create annotations");

        var task = await _taskRepository.GetByIdAsync(taskId);
        if (task == null)
            throw new KeyNotFoundException("Task not found");

        if (task.Status != LabelingTaskStatus.Opened)
            throw new InvalidOperationException("Task is not in a state that allows annotations.");

        var assignment = await _assignmentRepository.GetByTaskIdAndUserAsync(taskId, currentUserId);
        if (assignment == null)
            throw new UnauthorizedAccessException("You are not assigned to this task.");

        var existing = await _annotationRepository.GetByTaskIdAndAnnotatorIdAsync(taskId, currentUserId);
        if (existing != null)
            throw new InvalidOperationException("You have already submitted an annotation for this task.");

        var annotation = new Annotation
        {
            AnnotationId = Guid.NewGuid(),
            TaskId = taskId,
            AnnotatorId = currentUserId,
            Payload = payloadJson,
            SubmittedAt = DateTime.UtcNow
        };

        await _annotationRepository.AddAsync(annotation);
        await _annotationRepository.SaveChangesAsync();

        return annotation;
    }
}
