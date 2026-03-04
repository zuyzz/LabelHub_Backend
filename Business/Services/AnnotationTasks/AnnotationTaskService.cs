using DataLabelProject.Business.Models;
using DataLabelProject.Data.Repositories.Abstractions;
using DataLabel_Project_BE.DTOs.AnnotationTask;

namespace DataLabelProject.Business.Services.AnnotationTasks;

public class AnnotationTaskService : IAnnotationTaskService
{
    private readonly IAnnotationTaskRepository _annotationTaskRepository;

    public AnnotationTaskService(IAnnotationTaskRepository annotationTaskRepository)
    {
        _annotationTaskRepository = annotationTaskRepository;
    }

    public async Task<(TaskResponse? Task, string? ErrorMessage)> CreateTask(CreateTaskRequest request, Guid managerId)
    {
        var datasetItem = await _annotationTaskRepository.GetDatasetItemByIdAsync(request.DatasetItemId);
        if (datasetItem == null)
        {
            return (null, "Dataset item not found");
        }

        var dataset = await _annotationTaskRepository.GetDatasetByIdAsync(datasetItem.DatasetId);
        if (dataset == null)
        {
            return (null, "Dataset not found");
        }

        var project = await _annotationTaskRepository.GetProjectByIdAsync(dataset.ProjectId);
        if (project?.Status?.ToLower() == "archived")
        {
            return (null, "Cannot create task for archived project");
        }

        var deadlineAt = request.DeadlineAt;
        if (deadlineAt == null)
        {
            var systemConfig = await _annotationTaskRepository.GetSystemConfigAsync();
            int defaultDeadlineDays = systemConfig?.AnnotateDeadlineConfig ?? 7;
            deadlineAt = DateTime.UtcNow.AddDays(defaultDeadlineDays);
        }

        var task = new AnnotationTask
        {
            TaskId = Guid.NewGuid(),
            DatasetItemId = request.DatasetItemId,
            ScopeUri = request.ScopeUri,
            Status = "pending",
            Consensus = string.IsNullOrEmpty(request.Consensus) ? null : System.Text.Json.JsonSerializer.Serialize(new { type = request.Consensus }),
            DeadlineAt = deadlineAt,
            CreatedAt = DateTime.UtcNow
        };

        _annotationTaskRepository.AddTask(task);
        await _annotationTaskRepository.SaveChangesAsync();

        var activityLog = new ActivityLog
        {
            ActivityLogId = Guid.NewGuid(),
            ProjectId = dataset.ProjectId,
            UserId = managerId,
            EventType = "TASK_CREATED",
            TargetEntity = "Task",
            TargetId = task.TaskId,
            Details = System.Text.Json.JsonSerializer.Serialize(new
            {
                taskId = task.TaskId.ToString(),
                datasetItemId = task.DatasetItemId.ToString(),
                datasetId = dataset.DatasetId.ToString(),
                datasetName = dataset.Name,
                projectId = dataset.ProjectId.ToString(),
                scopeUri = task.ScopeUri,
                deadline = task.DeadlineAt?.ToString("yyyy-MM-dd HH:mm:ss"),
                consensusType = task.Consensus ?? "none"
            }),
            CreatedAt = DateTime.UtcNow
        };

        _annotationTaskRepository.AddActivityLog(activityLog);
        await _annotationTaskRepository.SaveChangesAsync();

        return (new TaskResponse
        {
            TaskId = task.TaskId,
            DatasetItemId = task.DatasetItemId,
            DatasetItemName = datasetItem.MediaType ?? "Unknown",
            ScopeUri = task.ScopeUri,
            Status = task.Status,
            Consensus = task.Consensus,
            DeadlineAt = task.DeadlineAt,
            AssignedAt = task.AssignedAt,
            CreatedAt = task.CreatedAt,
            Assignments = new List<AssignmentInfo>()
        }, null);
    }

    public async Task<(AssignmentInfo? Assignment, string? ErrorMessage)> AssignTask(Guid taskId, Guid annotatorId, Guid managerId)
    {
        var task = await _annotationTaskRepository.GetTaskWithDatasetItemAsync(taskId);
        if (task == null)
        {
            return (null, "Task not found");
        }

        if (task.Status == "completed")
        {
            return (null, "Cannot assign completed task");
        }

        var existingAssignment = await _annotationTaskRepository.HasAssignmentAsync(taskId);
        if (existingAssignment)
        {
            return (null, "Task already has an active assignment. Only one assignment per task is allowed");
        }

        var annotator = await _annotationTaskRepository.GetUserByIdAsync(annotatorId);
        if (annotator == null)
        {
            return (null, "Annotator not found");
        }

        if (!annotator.IsActive)
        {
            return (null, "Cannot assign task to inactive user");
        }

        var role = await _annotationTaskRepository.GetRoleByIdAsync(annotator.RoleId);
        if (role == null || role.RoleName.ToLower() != "annotator")
        {
            return (null, "User must have Annotator role");
        }

        var manager = await _annotationTaskRepository.GetUserByIdAsync(managerId);

        var assignment = new Assignment
        {
            AssignmentId = Guid.NewGuid(),
            TaskId = taskId,
            UserId = annotatorId,
            AssignedBy = managerId,
            AssignedAt = DateTime.UtcNow
        };

        _annotationTaskRepository.AddAssignment(assignment);
        task.Status = "in_progress";
        task.AssignedAt = DateTime.UtcNow;
        await _annotationTaskRepository.SaveChangesAsync();

        var projectId = await _annotationTaskRepository.GetProjectIdByDatasetItemIdAsync(task.DatasetItemId);
        var activityLog = new ActivityLog
        {
            ActivityLogId = Guid.NewGuid(),
            ProjectId = projectId,
            UserId = managerId,
            EventType = "TASK_ASSIGNED",
            TargetEntity = "Assignment",
            TargetId = assignment.AssignmentId,
            Details = System.Text.Json.JsonSerializer.Serialize(new
            {
                taskId,
                annotatorId,
                annotatorUsername = annotator.Username,
                assignedBy = managerId,
                assignedByUsername = manager?.Username
            }),
            CreatedAt = DateTime.UtcNow
        };

        _annotationTaskRepository.AddActivityLog(activityLog);
        await _annotationTaskRepository.SaveChangesAsync();

        return (new AssignmentInfo
        {
            AssignmentId = assignment.AssignmentId,
            UserId = annotator.UserId,
            Username = annotator.Username,
            DisplayName = annotator.DisplayName ?? annotator.Username,
            AssignedBy = managerId,
            AssignedByName = manager?.DisplayName ?? manager?.Username ?? "Unknown",
            AssignedAt = assignment.AssignedAt
        }, null);
    }

    public async Task<List<TaskResponse>> GetTasksForManager(Guid managerId)
    {
        var tasks = await _annotationTaskRepository.GetTasksForManagerAsync();
        return tasks.Select(MapToTaskResponse).ToList();
    }

    public async Task<List<TaskResponse>> GetTasksForAnnotator(Guid annotatorId)
    {
        var tasks = await _annotationTaskRepository.GetTasksForAnnotatorAsync(annotatorId);
        return tasks.Select(MapToTaskResponse).ToList();
    }

    public async Task<TaskResponse?> GetTaskById(Guid taskId)
    {
        var task = await _annotationTaskRepository.GetTaskByIdWithRelationsAsync(taskId);
        return task == null ? null : MapToTaskResponse(task);
    }

    public async Task<(TaskResponse? Task, string? ErrorMessage)> UpdateTaskStatus(Guid taskId, string newStatus, Guid userId, string userRole)
    {
        var task = await _annotationTaskRepository.GetTaskWithAssignmentsAsync(taskId);
        if (task == null)
        {
            return (null, "Task not found");
        }

        var validTransitions = new Dictionary<string, List<string>>
        {
            ["pending"] = new List<string> { "in_progress", "rejected" },
            ["in_progress"] = new List<string> { "completed", "rejected" },
            ["completed"] = new List<string> { "pending" },
            ["rejected"] = new List<string> { "pending" }
        };

        if (!validTransitions.ContainsKey(task.Status))
        {
            return (null, $"Invalid current status: {task.Status}");
        }

        if (!validTransitions[task.Status].Contains(newStatus))
        {
            return (null, $"Cannot change status from {task.Status} to {newStatus}");
        }

        if (userRole.ToLower() == "annotator")
        {
            if (newStatus != "completed")
            {
                return (null, "Annotator can only mark tasks as completed");
            }

            var isAssigned = task.Assignments.Any(a => a.UserId == userId);
            if (!isAssigned)
            {
                return (null, "You are not assigned to this task");
            }
        }

        var oldStatus = task.Status;
        task.Status = newStatus;
        await _annotationTaskRepository.SaveChangesAsync();

        var projectId = await _annotationTaskRepository.GetProjectIdByDatasetItemIdAsync(task.DatasetItemId);
        var activityLog = new ActivityLog
        {
            ActivityLogId = Guid.NewGuid(),
            ProjectId = projectId,
            UserId = userId,
            EventType = "TASK_STATUS_UPDATED",
            TargetEntity = "Task",
            TargetId = taskId,
            Details = System.Text.Json.JsonSerializer.Serialize(new
            {
                taskId,
                oldStatus,
                newStatus,
                userRole
            }),
            CreatedAt = DateTime.UtcNow
        };

        _annotationTaskRepository.AddActivityLog(activityLog);
        await _annotationTaskRepository.SaveChangesAsync();

        return (MapToTaskResponse(task), null);
    }

    public async Task<(TaskResponse? Task, string? ErrorMessage)> ReopenTask(Guid taskId, Guid managerId, string? reason)
    {
        var task = await _annotationTaskRepository.GetTaskWithAssignmentsAsync(taskId);
        if (task == null)
        {
            return (null, "Task not found");
        }

        if (task.Status != "completed" && task.Status != "rejected")
        {
            return (null, $"Cannot reopen task with status '{task.Status}'. Only completed or rejected tasks can be reopened");
        }

        var oldStatus = task.Status;
        task.Status = "pending";
        task.AssignedAt = null;

        var existingAssignments = await _annotationTaskRepository.GetAssignmentsByTaskIdAsync(taskId);
        _annotationTaskRepository.RemoveAssignments(existingAssignments);
        await _annotationTaskRepository.SaveChangesAsync();

        var projectId = await _annotationTaskRepository.GetProjectIdByDatasetItemIdAsync(task.DatasetItemId);
        var activityLog = new ActivityLog
        {
            ActivityLogId = Guid.NewGuid(),
            ProjectId = projectId,
            UserId = managerId,
            EventType = "TASK_REOPENED",
            TargetEntity = "Task",
            TargetId = taskId,
            Details = System.Text.Json.JsonSerializer.Serialize(new
            {
                taskId,
                oldStatus,
                newStatus = "pending",
                reason,
                managerId,
                removedAssignmentsCount = existingAssignments.Count
            }),
            CreatedAt = DateTime.UtcNow
        };

        _annotationTaskRepository.AddActivityLog(activityLog);
        await _annotationTaskRepository.SaveChangesAsync();

        return (MapToTaskResponse(task), null);
    }

    public async Task<(bool success, string? errorMessage)> DeleteTask(Guid taskId, Guid managerId)
    {
        var task = await _annotationTaskRepository.GetTaskWithAssignmentsAsync(taskId);
        if (task == null)
        {
            return (false, "Task not found or already deleted");
        }

        if (task.Status != "pending")
        {
            return (false, "Cannot delete task that is assigned, in progress, or completed. Use reopen instead.");
        }

        task.Deleted = true;

        if (task.Assignments.Any())
        {
            _annotationTaskRepository.RemoveAssignments(task.Assignments);
        }

        await _annotationTaskRepository.SaveChangesAsync();

        var projectId = await _annotationTaskRepository.GetProjectIdByDatasetItemIdAsync(task.DatasetItemId);
        var activityLog = new ActivityLog
        {
            ActivityLogId = Guid.NewGuid(),
            ProjectId = projectId,
            UserId = managerId,
            EventType = "TASK_DELETED",
            TargetEntity = "Task",
            TargetId = taskId,
            Details = System.Text.Json.JsonSerializer.Serialize(new
            {
                taskId = taskId.ToString(),
                datasetItemId = task.DatasetItemId.ToString(),
                status = task.Status,
                deletedBy = managerId.ToString(),
                deletedAt = DateTime.UtcNow
            }),
            CreatedAt = DateTime.UtcNow
        };

        _annotationTaskRepository.AddActivityLog(activityLog);
        await _annotationTaskRepository.SaveChangesAsync();

        return (true, null);
    }

    private static TaskResponse MapToTaskResponse(AnnotationTask task)
    {
        return new TaskResponse
        {
            TaskId = task.TaskId,
            DatasetItemId = task.DatasetItemId,
            DatasetItemName = task.TaskDatasetItem?.MediaType ?? "Unknown",
            ScopeUri = task.ScopeUri,
            Status = task.Status,
            Consensus = task.Consensus,
            DeadlineAt = task.DeadlineAt,
            AssignedAt = task.AssignedAt,
            CreatedAt = task.CreatedAt,
            Assignments = task.Assignments?.Select(a => new AssignmentInfo
            {
                AssignmentId = a.AssignmentId,
                UserId = a.UserId,
                Username = a.AssignmentUser?.Username ?? "Unknown",
                DisplayName = a.AssignmentUser?.DisplayName ?? a.AssignmentUser?.Username ?? "Unknown",
                AssignedBy = a.AssignedBy,
                AssignedByName = a.AssignedByUser?.DisplayName ?? a.AssignedByUser?.Username ?? "Unknown",
                AssignedAt = a.AssignedAt
            }).ToList()
        };
    }
}
