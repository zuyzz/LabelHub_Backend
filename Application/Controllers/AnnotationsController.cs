using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using DataLabelProject.Application.DTOs.Annotations;
using DataLabelProject.Business.Services.Annotations;

namespace DataLabelProject.Application.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class AnnotationsController : ControllerBase
{
    private readonly IAnnotationService _annotationService;

    public AnnotationsController(IAnnotationService annotationService)
    {
        _annotationService = annotationService;
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
    /// GET api/annotations/{id}
    /// Get annotation by ID
    /// </summary>
    [HttpGet("annotations/{id:guid}")]
    [Authorize(Roles = "admin,manager,annotator")]
    public async Task<IActionResult> GetAnnotationById(Guid id)
    {
        try
        {
            var result = await _annotationService.GetAnnotationByIdAsync(id, GetCurrentUserId(), GetCurrentUserRole());
            if (result == null)
                return NotFound(new { message = "Annotation not found" });
            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// GET api/tasks/items/{itemId}/annotations
    /// Get annotations by task item
    /// </summary>
    [HttpGet("tasks/items/{itemId:guid}/annotations")]
    [Authorize(Roles = "admin,manager,annotator")]
    public async Task<IActionResult> GetAnnotationsByTaskItem(Guid itemId, [FromQuery] AnnotationQueryParameters parameters)
    {
        try
        {
            var result = await _annotationService.GetAnnotationsByTaskItemAsync(
                itemId, parameters, GetCurrentUserId(), GetCurrentUserRole());
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// GET api/tasks/{taskId}/annotations
    /// Get annotations of a task
    /// </summary>
    [HttpGet("tasks/{taskId:guid}/annotations")]
    [Authorize(Roles = "admin,manager,annotator")]
    public async Task<IActionResult> GetAnnotationsByTask(Guid taskId, [FromQuery] AnnotationQueryParameters parameters)
    {
        try
        {
            var result = await _annotationService.GetAnnotationsByTaskAsync(
                taskId, parameters, GetCurrentUserId(), GetCurrentUserRole());
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// POST api/annotations/submit
    /// Submit annotations
    /// </summary>
    [HttpPost("annotations/submit")]
    [Authorize(Roles = "annotator")]
    public async Task<IActionResult> SubmitAnnotation([FromBody] List<SubmitAnnotationRequest> requests)
    {
        if (requests == null || requests.Count == 0)
            return BadRequest(new { message = "At least one annotation is required" });

        if (!ModelState.IsValid)
            return BadRequest(new { message = "Invalid input data", errors = ModelState });

        try
        {
            var result = await _annotationService.SubmitAnnotationsAsync(requests, GetCurrentUserId());
            return StatusCode(201, result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// PUT api/annotations/{id}
    /// Update annotation (conflict resolution)
    /// </summary>
    [HttpPut("annotations/{id:guid}")]
    [Authorize(Roles = "annotator")]
    public async Task<IActionResult> UpdateAnnotation(Guid id, [FromBody] UpdateAnnotationRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { message = "Invalid input data", errors = ModelState });

        try
        {
            var result = await _annotationService.UpdateAnnotationAsync(id, request, GetCurrentUserId());
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// POST api/annotations/skip
    /// Skip annotation
    /// </summary>
    [HttpPost("annotations/skip")]
    [Authorize(Roles = "annotator")]
    public async Task<IActionResult> SkipAnnotation([FromBody] SkipAnnotationRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { message = "Invalid input data", errors = ModelState });

        try
        {
            var result = await _annotationService.SkipAnnotationAsync(request, GetCurrentUserId());
            return StatusCode(201, result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
