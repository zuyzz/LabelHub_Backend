using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using DataLabelProject.Application.DTOs.Tasks;
using DataLabelProject.Business.Services.Tasks;

namespace DataLabelProject.Application.Controllers;

[ApiController]
[Route("api/tasks")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly ILabelingTaskService _taskService;

    public TasksController(ILabelingTaskService taskService)
    {
        _taskService = taskService;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    private string GetCurrentUserRole()
    {
        return User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
    }

    /// <summary>
    /// GET api/tasks/reviewer?status={status}&page={page}&pageSize={pageSize}
    /// Get active tasks for reviewer with filtering and pagination
    /// </summary>
    [HttpGet("reviewer")]
    [Authorize(Roles = "reviewer")]
    public async Task<IActionResult> GetTasksForReviewer([FromQuery] TaskQueryParameters @params)
    {
        try
        {
            var userId = GetCurrentUserId();
            var (tasks, totalCount) = await _taskService.GetTasksForReviewerAsync(
                userId, @params.Status, @params.Page, @params.PageSize);

            var response = tasks.Select(t => new TaskResponse
            {
                TaskId = t.TaskId,
                DatasetItemId = t.DatasetItemId,
                ProjectId = t.ProjectId,
                Status = t.Status.ToString(),
                RevisionCount = t.RevisionCount,
                Assignments = t.Assignments?.Select(a => new AssignmentResponse
                {
                    AssignmentId = a.AssignmentId,
                    TaskId = a.TaskId,
                    AssignedTo = a.AssignedTo,
                    AssignedBy = a.AssignedBy,
                    AssignedAt = a.AssignedAt,
                    StartedAt = a.StartedAt,
                    TimeLimitMinutes = a.TimeLimitMinutes,
                    Status = a.Status.ToString()
                }).ToList() ?? new List<AssignmentResponse>()
            }).ToList();

            return Ok(new
            {
                data = response,
                totalCount,
                page = @params.Page,
                pageSize = @params.PageSize
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// GET api/tasks/annotator?status={status}&page={page}&pageSize={pageSize}
    /// Get active tasks for annotator with filtering and pagination
    /// </summary>
    [HttpGet("annotator")]
    [Authorize(Roles = "annotator")]
    public async Task<IActionResult> GetTasksForAnnotator([FromQuery] TaskQueryParameters @params)
    {
        try
        {
            var userId = GetCurrentUserId();
            var (tasks, totalCount) = await _taskService.GetTasksForAnnotatorAsync(
                userId, @params.Status, @params.Page, @params.PageSize);

            var response = tasks.Select(t => new TaskResponse
            {
                TaskId = t.TaskId,
                DatasetItemId = t.DatasetItemId,
                ProjectId = t.ProjectId,
                Status = t.Status.ToString(),
                RevisionCount = t.RevisionCount,
                Assignments = t.Assignments?.Select(a => new AssignmentResponse
                {
                    AssignmentId = a.AssignmentId,
                    TaskId = a.TaskId,
                    AssignedTo = a.AssignedTo,
                    AssignedBy = a.AssignedBy,
                    AssignedAt = a.AssignedAt,
                    StartedAt = a.StartedAt,
                    TimeLimitMinutes = a.TimeLimitMinutes,
                    Status = a.Status.ToString()
                }).ToList() ?? new List<AssignmentResponse>()
            }).ToList();

            return Ok(new
            {
                data = response,
                totalCount,
                page = @params.Page,
                pageSize = @params.PageSize
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// GET api/tasks/{id}/reviewer
    /// Get task detail by ID for reviewer
    /// </summary>
    [HttpGet("{id}/reviewer")]
    [Authorize(Roles = "reviewer")]
    public async Task<IActionResult> GetTaskByIdForReviewer(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var task = await _taskService.GetTaskByIdForUserAsync(id, userId);

            if (task == null)
                return NotFound(new { message = "Task not found or not accessible" });

            var response = new TaskResponse
            {
                TaskId = task.TaskId,
                DatasetItemId = task.DatasetItemId,
                ProjectId = task.ProjectId,
                Status = task.Status.ToString(),
                RevisionCount = task.RevisionCount,
                Assignments = task.Assignments?.Select(a => new AssignmentResponse
                {
                    AssignmentId = a.AssignmentId,
                    TaskId = a.TaskId,
                    AssignedTo = a.AssignedTo,
                    AssignedBy = a.AssignedBy,
                    AssignedAt = a.AssignedAt,
                    StartedAt = a.StartedAt,
                    TimeLimitMinutes = a.TimeLimitMinutes,
                    Status = a.Status.ToString()
                }).ToList() ?? new List<AssignmentResponse>()
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// GET api/tasks/{id}/annotator
    /// Get task detail by ID for annotator
    /// </summary>
    [HttpGet("{id}/annotator")]
    [Authorize(Roles = "annotator")]
    public async Task<IActionResult> GetTaskByIdForAnnotator(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var task = await _taskService.GetTaskByIdForUserAsync(id, userId);

            if (task == null)
                return NotFound(new { message = "Task not found or not accessible" });

            var response = new TaskResponse
            {
                TaskId = task.TaskId,
                DatasetItemId = task.DatasetItemId,
                ProjectId = task.ProjectId,
                Status = task.Status.ToString(),
                RevisionCount = task.RevisionCount,
                Assignments = task.Assignments?.Select(a => new AssignmentResponse
                {
                    AssignmentId = a.AssignmentId,
                    TaskId = a.TaskId,
                    AssignedTo = a.AssignedTo,
                    AssignedBy = a.AssignedBy,
                    AssignedAt = a.AssignedAt,
                    StartedAt = a.StartedAt,
                    TimeLimitMinutes = a.TimeLimitMinutes,
                    Status = a.Status.ToString()
                }).ToList() ?? new List<AssignmentResponse>()
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// POST api/tasks/assign
    /// Bulk assign tasks from a dataset to a user
    /// </summary>
    [HttpPost("assign")]
    [Authorize(Roles = "manager")]
    public async Task<IActionResult> BulkAssignTasks([FromBody] BulkAssignTaskRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { message = "Invalid input data", errors = ModelState });

        try
        {
            var assignments = await _taskService.BulkAssignTasksAsync(
                request.DatasetId, request.ProjectId, request.AssignedTo, GetCurrentUserId());

            var response = assignments.Select(a => new AssignmentResponse
            {
                AssignmentId = a.AssignmentId,
                TaskId = a.TaskId,
                AssignedTo = a.AssignedTo,
                AssignedBy = a.AssignedBy,
                AssignedAt = a.AssignedAt,
                StartedAt = a.StartedAt,
                TimeLimitMinutes = a.TimeLimitMinutes,
                Status = a.Status.ToString()
            }).ToList();

            return StatusCode(201, new
            {
                message = $"Successfully assigned {assignments.Count} tasks",
                data = response
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// PUT api/tasks/assign/{id}
    /// Update time limit for assignments by dataset
    /// </summary>
    [HttpPut("assign/{id}")]
    [Authorize(Roles = "manager")]
    public async Task<IActionResult> UpdateAssignmentByDataset(Guid id, [FromBody] UpdateAssignmentByIdRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { message = "Invalid input data", errors = ModelState });

        try
        {
            var assignments = await _taskService.UpdateAssignmentsByDatasetAsync(
                id, request.DatasetId, request.TimeLimitMinutes);

            var response = assignments.Select(a => new AssignmentResponse
            {
                AssignmentId = a.AssignmentId,
                TaskId = a.TaskId,
                AssignedTo = a.AssignedTo,
                AssignedBy = a.AssignedBy,
                AssignedAt = a.AssignedAt,
                StartedAt = a.StartedAt,
                TimeLimitMinutes = a.TimeLimitMinutes,
                Status = a.Status.ToString()
            }).ToList();

            return Ok(new
            {
                message = $"Successfully updated {assignments.Count} assignments",
                data = response
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
