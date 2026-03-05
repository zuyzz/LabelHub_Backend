using DataLabelProject.Business.Models;
using DataLabelProject.Data.Repositories.Abstractions;
using DataLabel_Project_BE.DTOs.Annotation;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace DataLabelProject.Business.Services.Annotations;

public class AnnotationService : IAnnotationService
{
    private readonly IAnnotationWorkflowRepository _repository;

    public AnnotationService(IAnnotationWorkflowRepository repository)
    {
        _repository = repository;
    }

    public async Task<(AnnotationResponse? Annotation, string? ErrorMessage)> SaveDraftAsync(Guid taskId, Guid annotatorId, SaveDraftRequest request)
    {
        if (!IsValidJson(request.AnnotationPayload))
        {
            return (null, "annotationPayload must be valid JSON");
        }

        var task = await _repository.GetTaskWithDatasetContextAsync(taskId);
        if (task == null)
        {
            return (null, "Task not found");
        }

        if (!task.Assignments.Any(a => a.UserId == annotatorId))
        {
            return (null, "You are not assigned to this task");
        }

        var labelSet = await _repository.GetLatestLabelSetAsync();
        if (labelSet == null)
        {
            return (null, "Dataset has no active LabelSet");
        }

        var annotation = await _repository.GetAnnotationByTaskAndAnnotatorAsync(taskId, annotatorId);
        if (annotation == null)
        {
            annotation = new Annotation
            {
                AnnotationId = Guid.NewGuid(),
                TaskId = taskId,
                AnnotatorId = annotatorId
            };
            _repository.AddAnnotation(annotation);
        }

        annotation.LabelSetId = labelSet.LabelSetId;
        annotation.LabelSetVersionNumber = labelSet.VersionNumber;
        annotation.AnnotationPayload = request.AnnotationPayload;
        annotation.IsDraft = true;
        annotation.SubmittedAt = null;

        await _repository.SaveChangesAsync();
        return (Map(annotation), null);
    }

    public async Task<(AnnotationResponse? Annotation, string? ErrorMessage)> SubmitAsync(Guid taskId, Guid annotatorId, SubmitAnnotationRequest request)
    {
        if (!IsValidJson(request.AnnotationPayload))
        {
            return (null, "annotationPayload must be valid JSON");
        }

        var task = await _repository.GetTaskWithDatasetContextAsync(taskId);
        if (task == null)
        {
            return (null, "Task not found");
        }

        if (!task.Assignments.Any(a => a.UserId == annotatorId))
        {
            return (null, "You are not assigned to this task");
        }

        var labelSet = await _repository.GetLatestLabelSetAsync();
        if (labelSet == null)
        {
            return (null, "Dataset has no active LabelSet");
        }

        if (!ValidateRequiredFields(request.AnnotationPayload, request.RequiredFields))
        {
            return (null, "Required fields are missing or empty");
        }

        var annotation = await _repository.GetAnnotationByTaskAndAnnotatorAsync(taskId, annotatorId);
        if (annotation == null)
        {
            annotation = new Annotation
            {
                AnnotationId = Guid.NewGuid(),
                TaskId = taskId,
                AnnotatorId = annotatorId
            };
            _repository.AddAnnotation(annotation);
        }

        annotation.LabelSetId = labelSet.LabelSetId;
        annotation.LabelSetVersionNumber = labelSet.VersionNumber;
        annotation.AnnotationPayload = request.AnnotationPayload;
        annotation.IsDraft = false;
        annotation.SubmittedAt = DateTime.UtcNow;

        task.Status = "completed";

        var payloadHash = ComputeSha256(request.AnnotationPayload);
        var projectId = await _repository.GetProjectIdByTaskIdAsync(taskId);
        if (projectId != Guid.Empty)
        {
            _repository.AddActivityLog(new ActivityLog
            {
                ActivityLogId = Guid.NewGuid(),
                ProjectId = projectId,
                UserId = annotatorId,
                EventType = "ANNOTATION_SUBMIT",
                TargetEntity = "Annotation",
                TargetId = annotation.AnnotationId,
                Details = JsonSerializer.Serialize(new
                {
                    annotationId = annotation.AnnotationId,
                    taskId,
                    submittedAt = annotation.SubmittedAt,
                    payloadHash
                }),
                CreatedAt = DateTime.UtcNow
            });
        }

        await _repository.SaveChangesAsync();
        return (Map(annotation), null);
    }

    private static bool IsValidJson(string payload)
    {
        try
        {
            JsonDocument.Parse(payload);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool ValidateRequiredFields(string payload, List<string>? requiredFields)
    {
        if (requiredFields == null || requiredFields.Count == 0)
        {
            return true;
        }

        using var doc = JsonDocument.Parse(payload);
        foreach (var field in requiredFields)
        {
            if (!doc.RootElement.TryGetProperty(field, out var value))
            {
                return false;
            }

            if (value.ValueKind == JsonValueKind.Null)
            {
                return false;
            }

            if (value.ValueKind == JsonValueKind.String && string.IsNullOrWhiteSpace(value.GetString()))
            {
                return false;
            }
        }

        return true;
    }

    private static string ComputeSha256(string input)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(hash);
    }

    private static AnnotationResponse Map(Annotation annotation) => new()
    {
        AnnotationId = annotation.AnnotationId,
        TaskId = annotation.TaskId,
        AnnotatorId = annotation.AnnotatorId,
        LabelSetId = annotation.LabelSetId,
        LabelSetVersionNumber = annotation.LabelSetVersionNumber,
        AnnotationPayload = annotation.AnnotationPayload,
        IsDraft = annotation.IsDraft,
        SubmittedAt = annotation.SubmittedAt
    };
}
