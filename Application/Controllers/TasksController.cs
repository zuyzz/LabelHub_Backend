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
    /// Get tasks based on current user's role.
    /// admin/manager: all tasks. reviewer/annotator: active tasks assigned to them.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetTasks()
    {
        var currentUserId = GetCurrentUserId();
        var currentUserRole = GetCurrentUserRole();

        var tasks = await _taskService.GetTasksForUserAsync(currentUserId, currentUserRole);

        var response = tasks.Select(t => new TaskResponse
        {
            TaskId = t.TaskId,
            DatasetItemId = t.DatasetItemId,
            ProjectId = t.ProjectId,
            Assignments = t.Assignments.Select(a => new AssignmentResponse
            {
                AssignmentId = a.AssignmentId,
                TaskId = a.TaskId,
                AssignedTo = a.AssignedTo,
                AssignedBy = a.AssignedBy,
                AssignedAt = a.AssignedAt,
                DeadlineAt = a.DeadlineAt,
                Status = a.Status.ToString()
            }).ToList()
        }).ToList();

        return Ok(response);
    }

    /// <summary>
    /// Create a new labeling task.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateTask([FromBody] CreateTaskRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { message = "Invalid input data", errors = ModelState });

        try
        {
            var task = await _taskService.CreateTaskAsync(request.DatasetItemId, request.ProjectId);

            var response = new TaskResponse
            {
                TaskId = task.TaskId,
                DatasetItemId = task.DatasetItemId,
                ProjectId = task.ProjectId
            };

            return CreatedAtAction(nameof(GetTasks), new { }, response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Assign a task to a reviewer or annotator (manager only).
    /// </summary>
    [HttpPost("assign")]
    [Authorize(Roles = "manager")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AssignTask([FromBody] AssignTaskRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { message = "Invalid input data", errors = ModelState });

        try
        {
            var assignedBy = GetCurrentUserId();
            var assignment = await _taskService.AssignTaskAsync(
                request.TaskId,
                request.ProjectId,
                request.AssignedTo,
                assignedBy);

            var response = new AssignmentResponse
            {
                AssignmentId = assignment.AssignmentId,
                TaskId = assignment.TaskId,
                AssignedTo = assignment.AssignedTo,
                AssignedBy = assignment.AssignedBy,
                AssignedAt = assignment.AssignedAt,
                DeadlineAt = assignment.DeadlineAt,
                Status = assignment.Status.ToString()
            };

            return StatusCode(StatusCodes.Status201Created, response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update the deadline of an existing assignment (manager only).
    /// </summary>
    [HttpPut("assign")]
    [Authorize(Roles = "manager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateDeadline([FromBody] UpdateAssignmentDeadlineRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { message = "Invalid input data", errors = ModelState });

        try
        {
            var assignment = await _taskService.UpdateDeadlineAsync(request.TaskId, request.DeadlineAt);

            var response = new AssignmentResponse
            {
                AssignmentId = assignment.AssignmentId,
                TaskId = assignment.TaskId,
                AssignedTo = assignment.AssignedTo,
                AssignedBy = assignment.AssignedBy,
                AssignedAt = assignment.AssignedAt,
                DeadlineAt = assignment.DeadlineAt,
                Status = assignment.Status.ToString()
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
