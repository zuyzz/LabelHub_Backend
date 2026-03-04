using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DataLabel_Project_BE.DTOs.AnnotationTask;
using DataLabelProject.Business.Services.AnnotationTasks;
using System.Security.Claims;

namespace DataLabel_Project_BE.Controllers;

/// <summary>
/// Annotation Task Management
/// </summary>
[ApiController]
[Route("api/tasks")]
[Authorize]
public class AnnotationTasksController : ControllerBase
{
    private readonly IAnnotationTaskService _taskService;

    public AnnotationTasksController(IAnnotationTaskService taskService)
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
        return User.FindFirst(ClaimTypes.Role)?.Value ?? "";
    }

    /// <summary>
    /// Create new annotation task (Manager only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "admin,manager")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TaskResponse>> CreateTask([FromBody] CreateTaskRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { message = "Invalid input data", errors = ModelState });

        var managerId = GetCurrentUserId();
        var (task, errorMessage) = await _taskService.CreateTask(request, managerId);

        if (task == null)
            return BadRequest(new { message = errorMessage });

        return CreatedAtAction(nameof(GetTaskById), new { id = task.TaskId }, new
        {
            message = "Task created successfully",
            data = task
        });
    }

    /// <summary>
    /// Assign task to annotator (Manager only)
    /// </summary>
    [HttpPost("{id}/assign")]
    [Authorize(Roles = "admin,manager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> AssignTask(Guid id, [FromBody] AssignTaskRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { message = "Invalid input data", errors = ModelState });

        var managerId = GetCurrentUserId();
        var (assignment, errorMessage) = await _taskService.AssignTask(id, request.AnnotatorId, managerId);

        if (assignment == null)
            return BadRequest(new { message = errorMessage });

        return Ok(new
        {
            message = "Task assigned successfully",
            data = assignment
        });
    }

    /// <summary>
    /// Get all tasks (Manager: all, Annotator: assigned only)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<TaskResponse>>> GetTasks()
    {
        var userId = GetCurrentUserId();
        var userRole = GetCurrentUserRole();

        List<TaskResponse> tasks;

        if (userRole.ToLower() == "annotator")
        {
            tasks = await _taskService.GetTasksForAnnotator(userId);
        }
        else
        {
            tasks = await _taskService.GetTasksForManager(userId);
        }

        if (tasks.Count == 0)
        {
            return Ok(new { message = "No tasks found", data = tasks });
        }

        return Ok(new
        {
            message = "Tasks retrieved successfully",
            count = tasks.Count,
            data = tasks
        });
    }

    /// <summary>
    /// Get task by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskResponse>> GetTaskById(Guid id)
    {
        var task = await _taskService.GetTaskById(id);
        if (task == null)
            return NotFound(new { message = "Task not found" });

        return Ok(new { message = "Task retrieved successfully", data = task });
    }

    /// <summary>
    /// Update task status
    /// </summary>
    [HttpPatch("{id}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskResponse>> UpdateTaskStatus(Guid id, [FromBody] UpdateTaskStatusRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { message = "Invalid input data", errors = ModelState });

        var userId = GetCurrentUserId();
        var userRole = GetCurrentUserRole();

        var (task, errorMessage) = await _taskService.UpdateTaskStatus(id, request.Status, userId, userRole);

        if (task == null)
        {
            if (errorMessage == "Task not found")
                return NotFound(new { message = errorMessage });

            return BadRequest(new { message = errorMessage });
        }

        return Ok(new
        {
            message = "Task status updated successfully",
            data = task
        });
    }

    /// <summary>
    /// Reopen task (Manager only) - BR-28b
    /// </summary>
    [HttpPost("{id}/reopen")]
    [Authorize(Roles = "admin,manager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskResponse>> ReopenTask(Guid id, [FromBody] ReopenTaskRequest? request = null)
    {
        var managerId = GetCurrentUserId();
        var reason = request?.Reason;

        var (task, errorMessage) = await _taskService.ReopenTask(id, managerId, reason);

        if (task == null)
        {
            if (errorMessage == "Task not found")
                return NotFound(new { message = errorMessage });

            return BadRequest(new { message = errorMessage });
        }

        return Ok(new
        {
            message = "Task reopened successfully. Assignments removed and task reset to pending status.",
            data = task
        });
    }

    /// <summary>
    /// Delete task (Manager only) - Soft delete for pending tasks only
    /// </summary>
    [HttpDelete("{taskId}")]
    [Authorize(Roles = "admin,manager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteTask(Guid taskId)
    {
        var managerId = GetCurrentUserId();
        var (success, errorMessage) = await _taskService.DeleteTask(taskId, managerId);

        if (!success)
        {
            if (errorMessage?.Contains("not found") == true)
                return NotFound(new { message = errorMessage });

            return BadRequest(new { message = errorMessage });
        }

        return Ok(new { message = "Task deleted successfully" });
    }}
