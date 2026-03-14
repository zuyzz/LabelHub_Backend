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

    [HttpGet]
    public async Task<IActionResult> GetTasks()
    {
        var tasks = await _taskService.GetTasksForUserAsync(GetCurrentUserId(), GetCurrentUserRole());

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
                StartedAt = a.StartedAt,
                TimeLimitMinutes = a.TimeLimitMinutes,
                Status = a.Status.ToString()
            }).ToList()
        }).ToList();

        return Ok(response);
    }

    [HttpPost("assign")]
    [Authorize(Roles = "manager")]
    public async Task<IActionResult> AssignTask([FromBody] AssignTaskRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { message = "Invalid input data", errors = ModelState });

        try
        {
            var assignment = await _taskService.AssignTaskAsync(
                request.TaskId, request.ProjectId, request.AssignedTo, GetCurrentUserId());

            var response = new AssignmentResponse
            {
                AssignmentId = assignment.AssignmentId,
                TaskId = assignment.TaskId,
                AssignedTo = assignment.AssignedTo,
                AssignedBy = assignment.AssignedBy,
                AssignedAt = assignment.AssignedAt,
                StartedAt = assignment.StartedAt,
                TimeLimitMinutes = assignment.TimeLimitMinutes,
                Status = assignment.Status.ToString()
            };
            return StatusCode(201, response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("assign")]
    [Authorize(Roles = "manager")]
    public async Task<IActionResult> UpdateTimeLimit([FromBody] UpdateAssignmentTimeLimitRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { message = "Invalid input data", errors = ModelState });

        try
        {
            var assignment = await _taskService.UpdateTimeLimitAsync(request.TaskId, request.TimeLimitMinutes);
            var response = new AssignmentResponse
            {
                AssignmentId = assignment.AssignmentId,
                TaskId = assignment.TaskId,
                AssignedTo = assignment.AssignedTo,
                AssignedBy = assignment.AssignedBy,
                AssignedAt = assignment.AssignedAt,
                StartedAt = assignment.StartedAt,
                TimeLimitMinutes = assignment.TimeLimitMinutes,
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
