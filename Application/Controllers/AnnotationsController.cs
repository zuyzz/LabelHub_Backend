using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using DataLabelProject.Application.DTOs.Annotations;
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
    [Authorize(Roles = "admin,manager,annotator")]
    public async Task<IActionResult> GetAnnotations([FromQuery] string? status)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var currentUserRole = GetCurrentUserRole();

            var annotations = await _annotationService.GetAnnotationsForUserAsync(currentUserId, currentUserRole, status);

            var response = annotations.Select(a => {
                AnnotationPayload? parsedPayload = null;
                try
                {
                    parsedPayload = System.Text.Json.JsonSerializer.Deserialize<AnnotationPayload>(a.Payload);
                }
                catch
                {
                    parsedPayload = null;
                }

                return new AnnotationResponse
                {
                    AnnotationId = a.AnnotationId,
                    TaskId = a.TaskItemId,
                    AnnotatorId = a.AnnotatorId,
                    Payload = parsedPayload ?? (object)a.Payload,
                    SubmittedAt = a.SubmittedAt,
                    Status = a.Reviews.OrderByDescending(r => r.ReviewedAt).FirstOrDefault()?.Result.ToString().ToLower()
                };
            }).ToList();

            return Ok(response);
        }
        catch (ArgumentException)
        {
            return BadRequest(new { message = "Invalid status filter." });
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

    [HttpPost]
    [Authorize(Roles = "annotator")]
    public async Task<IActionResult> CreateAnnotation([FromBody] CreateAnnotationRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { message = "Invalid input data", errors = ModelState });

        try
        {
            var currentUserId = GetCurrentUserId();
            var currentUserRole = GetCurrentUserRole();
            var annotation = await _annotationService.CreateAnnotationAsync(request, currentUserId, currentUserRole);

            var parsedPayload = (AnnotationPayload?)null;
            try
            {
                parsedPayload = System.Text.Json.JsonSerializer.Deserialize<AnnotationPayload>(annotation.Payload);
            }
            catch
            {
                parsedPayload = null;
            }

            var response = new AnnotationResponse
            {
                AnnotationId = annotation.AnnotationId,
                TaskId = annotation.TaskItemId,
                AnnotatorId = annotation.AnnotatorId,
                Payload = parsedPayload ?? (object)annotation.Payload,
                SubmittedAt = annotation.SubmittedAt,
                Status = null
            };

            return StatusCode(201, response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
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
}
