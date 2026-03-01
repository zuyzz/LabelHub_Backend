using Microsoft.EntityFrameworkCore;
using DataLabelProject.Data;
using DataLabelProject.Business.Models;
using DataLabel_Project_BE.DTOs.AnnotationTask;

namespace DataLabel_Project_BE.Services;

public class AnnotationTaskService
{
    private readonly AppDbContext _context;

    public AnnotationTaskService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Create new annotation task (Manager only)
    /// BR-23: AnnotationTask must belong to a DatasetItem
    /// BR-27: AnnotationTask deadline uses system default if not provided
    /// BR-05: Cannot create task for archived project
    /// BR-04: Log activity to ActivityLog
    /// </summary>
    public async Task<(TaskResponse? Task, string? ErrorMessage)> CreateTask(CreateTaskRequest request, Guid managerId)
    {
        // Verify dataset item exists and get project info (BR-23, BR-05)
        var datasetItem = await _context.Set<DatasetItem>()
            .AsNoTracking()
            .Where(di => di.ItemId == request.DatasetItemId)
            .Select(di => new { di.ItemId, di.DatasetId, di.MediaType })
            .FirstOrDefaultAsync();

        if (datasetItem == null)
        {
            return (null, "Dataset item not found");
        }

        // Get dataset and project info
        var dataset = await _context.Datasets
            .AsNoTracking()
            .Where(d => d.DatasetId == datasetItem.DatasetId)
            .Select(d => new { d.DatasetId, d.ProjectId, d.Name })
            .FirstOrDefaultAsync();

        if (dataset == null)
        {
            return (null, "Dataset not found");
        }

        // BR-05: Check if project is archived
        var project = await _context.Projects
            .AsNoTracking()
            .Where(p => p.ProjectId == dataset.ProjectId)
            .Select(p => new { p.ProjectId, p.Status })
            .FirstOrDefaultAsync();

        if (project?.Status?.ToLower() == "archived")
        {
            return (null, "Cannot create task for archived project");
        }

        // Get system default deadline if not provided (BR-27)
        var deadlineAt = request.DeadlineAt;
        if (deadlineAt == null)
        {
            var systemConfig = await _context.SystemConfigs.FirstOrDefaultAsync();
            int defaultDeadlineDays = systemConfig?.AnnotateDeadlineConfig ?? 7;
            deadlineAt = DateTime.UtcNow.AddDays(defaultDeadlineDays);
        }

        var task = new AnnotationTask
        {
            TaskId = Guid.NewGuid(),
            DatasetItemId = request.DatasetItemId,
            ScopeUri = request.ScopeUri,
            Status = "pending", // BR-24: Initial status
            Consensus = string.IsNullOrEmpty(request.Consensus) ? null : System.Text.Json.JsonSerializer.Serialize(new { type = request.Consensus }),
            DeadlineAt = deadlineAt,
            CreatedAt = DateTime.UtcNow
        };

        _context.AnnotationTasks.Add(task);
        await _context.SaveChangesAsync();

        // BR-04: Log activity
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
        _context.ActivityLogs.Add(activityLog);
        await _context.SaveChangesAsync();

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

    /// <summary>
    /// Assign task to annotator (Manager only)
    /// BR-25: Only one active assignment per task
    /// BR-26: Inactive users cannot be assigned tasks
    /// BR-04: Log activity to ActivityLog
    /// </summary>
    public async Task<(AssignmentInfo? Assignment, string? ErrorMessage)> AssignTask(Guid taskId, Guid annotatorId, Guid managerId)
    {
        // Verify task exists
        var task = await _context.AnnotationTasks
            .Include(t => t.TaskDatasetItem)
            .FirstOrDefaultAsync(t => t.TaskId == taskId && !t.Deleted);
        if (task == null)
        {
            return (null, "Task not found");
        }

        // BR-28: Submitted tasks cannot be reassigned
        if (task.Status == "completed")
        {
            return (null, "Cannot assign completed task");
        }

        // BR-25: Check if task already has active assignment
        var existingAssignment = await _context.Assignments
            .AnyAsync(a => a.TaskId == taskId);

        if (existingAssignment)
        {
            return (null, "Task already has an active assignment. Only one assignment per task is allowed");
        }

        // Verify annotator exists and is active (BR-26)
        var annotator = await _context.Users
            .FirstOrDefaultAsync(u => u.UserId == annotatorId);
        if (annotator == null)
        {
            return (null, "Annotator not found");
        }

        if (!annotator.IsActive)
        {
            return (null, "Cannot assign task to inactive user");
        }

        // Verify annotator has correct role
        var role = await _context.Roles.FindAsync(annotator.RoleId);
        if (role == null || role.RoleName.ToLower() != "annotator")
        {
            return (null, "User must have Annotator role");
        }

        // Get manager info
        var manager = await _context.Users.FindAsync(managerId);

        // Create assignment
        var assignment = new Assignment
        {
            AssignmentId = Guid.NewGuid(),
            TaskId = taskId,
            UserId = annotatorId,
            AssignedBy = managerId,
            AssignedAt = DateTime.UtcNow
        };

        _context.Assignments.Add(assignment);

        // Update task status and assigned time
        task.Status = "in_progress";
        task.AssignedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Get projectId for ActivityLog
        var projectId = await _context.DatasetItems
            .Where(di => di.ItemId == task.DatasetItemId)
            .Select(di => di.Dataset.ProjectId)
            .FirstOrDefaultAsync();

        // BR-04: Log activity
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
                taskId = taskId,
                annotatorId = annotatorId,
                annotatorUsername = annotator.Username,
                assignedBy = managerId,
                assignedByUsername = manager?.Username
            }),
            CreatedAt = DateTime.UtcNow
        };
        _context.ActivityLogs.Add(activityLog);
        await _context.SaveChangesAsync();

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

    /// <summary>
    /// Get all tasks for manager
    /// </summary>
    public async Task<List<TaskResponse>> GetTasksForManager(Guid managerId)
    {
        var tasks = await _context.AnnotationTasks
            .Where(t => !t.Deleted)
            .Include(t => t.TaskDatasetItem)
            .Include(t => t.Assignments)
                .ThenInclude(a => a.AssignmentUser)
            .Include(t => t.Assignments)
                .ThenInclude(a => a.AssignedByUser)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return tasks.Select(t => MapToTaskResponse(t)).ToList();
    }

    /// <summary>
    /// Get tasks assigned to annotator
    /// </summary>
    public async Task<List<TaskResponse>> GetTasksForAnnotator(Guid annotatorId)
    {
        var tasks = await _context.AnnotationTasks
            .Where(t => !t.Deleted)
            .Include(t => t.TaskDatasetItem)
            .Include(t => t.Assignments)
                .ThenInclude(a => a.AssignmentUser)
            .Include(t => t.Assignments)
                .ThenInclude(a => a.AssignedByUser)
            .Where(t => t.Assignments.Any(a => a.UserId == annotatorId))
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return tasks.Select(t => MapToTaskResponse(t)).ToList();
    }

    /// <summary>
    /// Get task by ID
    /// </summary>
    public async Task<TaskResponse?> GetTaskById(Guid taskId)
    {
        var task = await _context.AnnotationTasks
            .Include(t => t.TaskDatasetItem)
            .Include(t => t.Assignments)
                .ThenInclude(a => a.AssignmentUser)
            .Include(t => t.Assignments)
                .ThenInclude(a => a.AssignedByUser)
            .FirstOrDefaultAsync(t => t.TaskId == taskId && !t.Deleted);

        return task == null ? null : MapToTaskResponse(task);
    }

    /// <summary>
    /// Update task status
    /// BR-24: AnnotationTask must follow defined workflow statuses
    /// BR-04: Log activity to ActivityLog
    /// </summary>
    public async Task<(TaskResponse? Task, string? ErrorMessage)> UpdateTaskStatus(Guid taskId, string newStatus, Guid userId, string userRole)
    {
        var task = await _context.AnnotationTasks
            .Include(t => t.TaskDatasetItem)
            .Include(t => t.Assignments)
            .FirstOrDefaultAsync(t => t.TaskId == taskId && !t.Deleted);

        if (task == null)
        {
            return (null, "Task not found");
        }

        // Validate status transition (BR-24)
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

        // Check permissions
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
        await _context.SaveChangesAsync();

        // Get projectId for ActivityLog
        var projectId = await _context.DatasetItems
            .Where(di => di.ItemId == task.DatasetItemId)
            .Select(di => di.Dataset.ProjectId)
            .FirstOrDefaultAsync();

        // BR-04: Log activity
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
                taskId = taskId,
                oldStatus = oldStatus,
                newStatus = newStatus,
                userRole = userRole
            }),
            CreatedAt = DateTime.UtcNow
        };
        _context.ActivityLogs.Add(activityLog);
        await _context.SaveChangesAsync();

        return (MapToTaskResponse(task), null);
    }

    private TaskResponse MapToTaskResponse(AnnotationTask task)
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

    /// <summary>
    /// Reopen task (Manager only)
    /// BR-28b: Manager can reopen submitted/rejected tasks for reassignment
    /// BR-04: Log activity to ActivityLog
    /// </summary>
    public async Task<(TaskResponse? Task, string? ErrorMessage)> ReopenTask(Guid taskId, Guid managerId, string? reason)
    {
        var task = await _context.AnnotationTasks
            .Include(t => t.TaskDatasetItem)
            .Include(t => t.Assignments)
            .FirstOrDefaultAsync(t => t.TaskId == taskId && !t.Deleted);

        if (task == null)
        {
            return (null, "Task not found");
        }

        // Only allow reopening completed or rejected tasks
        if (task.Status != "completed" && task.Status != "rejected")
        {
            return (null, $"Cannot reopen task with status '{task.Status}'. Only completed or rejected tasks can be reopened");
        }

        var oldStatus = task.Status;

        // Reset task to pending status
        task.Status = "pending";
        task.AssignedAt = null;

        // Remove existing assignments to allow reassignment
        var existingAssignments = await _context.Assignments
            .Where(a => a.TaskId == taskId)
            .ToListAsync();
        _context.Assignments.RemoveRange(existingAssignments);

        await _context.SaveChangesAsync();

        // Get projectId for ActivityLog
        var projectId = await _context.DatasetItems
            .Where(di => di.ItemId == task.DatasetItemId)
            .Select(di => di.Dataset.ProjectId)
            .FirstOrDefaultAsync();

        // BR-04: Log activity
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
                taskId = taskId,
                oldStatus = oldStatus,
                newStatus = "pending",
                reason = reason,
                managerId = managerId,
                removedAssignmentsCount = existingAssignments.Count
            }),
            CreatedAt = DateTime.UtcNow
        };
        _context.ActivityLogs.Add(activityLog);
        await _context.SaveChangesAsync();

        return (MapToTaskResponse(task), null);
    }

    /// <summary>
    /// Soft delete task (Manager only)
    /// BR-29: Only pending tasks can be deleted (not yet assigned or in progress)
    /// </summary>
    public async Task<(bool success, string? errorMessage)> DeleteTask(Guid taskId, Guid managerId)
    {
        var task = await _context.AnnotationTasks
            .Include(t => t.TaskDatasetItem)
            .Include(t => t.Assignments)
            .FirstOrDefaultAsync(t => t.TaskId == taskId && !t.Deleted);

        if (task == null)
            return (false, "Task not found or already deleted");

        // BR-29: Only delete pending tasks (chưa ai làm)
        if (task.Status != "pending")
            return (false, "Cannot delete task that is assigned, in progress, or completed. Use reopen instead.");

        // Soft delete
        task.Deleted = true;

        // Remove assignments if any (should not have for pending, but just in case)
        if (task.Assignments.Any())
        {
            _context.Assignments.RemoveRange(task.Assignments);
        }

        await _context.SaveChangesAsync();

        // Get projectId for ActivityLog
        var projectId = await _context.DatasetItems
            .Where(di => di.ItemId == task.DatasetItemId)
            .Select(di => di.Dataset.ProjectId)
            .FirstOrDefaultAsync();

        // BR-04: Log activity
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

        _context.ActivityLogs.Add(activityLog);
        await _context.SaveChangesAsync();

        return (true, null);
    }}