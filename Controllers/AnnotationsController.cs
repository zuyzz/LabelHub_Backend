using DataLabelProject.Business.Services.Annotations;
using DataLabel_Project_BE.DTOs.Annotation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DataLabel_Project_BE.Controllers;

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

    [HttpPost("tasks/{taskId}/draft")]
    [Authorize(Roles = "annotator")]
    public async Task<ActionResult> SaveDraft(Guid taskId, [FromBody] SaveDraftRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { message = "Invalid input data", errors = ModelState });
        }

        var annotatorId = GetCurrentUserId();
        var (annotation, errorMessage) = await _annotationService.SaveDraftAsync(taskId, annotatorId, request);
        if (annotation == null)
        {
            return BadRequest(new { message = errorMessage });
        }

        return Ok(new { message = "Draft saved successfully", data = annotation });
    }

    [HttpPost("tasks/{taskId}/submit")]
    [Authorize(Roles = "annotator")]
    public async Task<ActionResult> Submit(Guid taskId, [FromBody] SubmitAnnotationRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { message = "Invalid input data", errors = ModelState });
        }

        var annotatorId = GetCurrentUserId();
        var (annotation, errorMessage) = await _annotationService.SubmitAsync(taskId, annotatorId, request);
        if (annotation == null)
        {
            return BadRequest(new { message = errorMessage });
        }

        return Ok(new { message = "Annotation submitted successfully", data = annotation });
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}
