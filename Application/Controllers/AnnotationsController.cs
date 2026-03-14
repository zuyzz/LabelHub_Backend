using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using DataLabelProject.Application.DTOs.Annotations;
using DataLabelProject.Business.Models;
using DataLabelProject.Business.Services.Annotations;

namespace DataLabelProject.Application.Controllers;

[ApiController]
[Route("api/annotations")]
[Authorize]
public class AnnotationsController : ControllerBase
{
    private readonly IAnnotationService _annotationService;

    public AnnotationsController(IAnnotationService annotationService)
    {
        _annotationService = annotationService;
    }

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    private string GetCurrentUserRole()
    {
        return User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
    }

    private static AnnotationResponse MapToResponse(Annotation a)
    {
        object payload;
        try
        {
            payload = JsonSerializer.Deserialize<AnnotationPayload>(a.Payload) ?? (object)a.Payload;
        }
        catch
        {
            payload = a.Payload;
        }

        return new AnnotationResponse
        {
            AnnotationId = a.AnnotationId,
            TaskId = a.TaskId,
            AnnotatorId = a.AnnotatorId,
            Payload = payload,
            SubmittedAt = a.SubmittedAt,
            Status = a.Reviews.OrderByDescending(r => r.ReviewedAt).FirstOrDefault()?.Result.ToString().ToLower()
        };
    }

    [HttpGet]
    [Authorize(Roles = "admin,manager,annotator")]
    public async Task<IActionResult> GetAnnotations([FromQuery] string? status)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
                return Unauthorized(new { message = "Invalid user identity." });

            var currentUserRole = GetCurrentUserRole();

            var annotations = await _annotationService.GetAnnotationsForUserAsync(currentUserId.Value, currentUserRole, status);

            var response = annotations.Select(MapToResponse).ToList();

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpPost]
    [Authorize(Roles = "annotator")]
    public async Task<IActionResult> CreateAnnotation([FromBody] CreateAnnotationRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { message = "Invalid input data", errors = ModelState });

        try
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
                return Unauthorized(new { message = "Invalid user identity." });

            var currentUserRole = GetCurrentUserRole();
            var payloadJson = JsonSerializer.Serialize(request.Payload);
            var annotation = await _annotationService.CreateAnnotationAsync(request.TaskId, payloadJson, currentUserId.Value, currentUserRole);

            var response = MapToResponse(annotation);
            response.Status = null;

            return StatusCode(201, response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }
}
