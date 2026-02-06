using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DataLabel_Project_BE.DTOs.Guideline;
using DataLabel_Project_BE.Services;

namespace DataLabel_Project_BE.Controllers;

/// <summary>
/// Guideline Management
/// </summary>
[ApiController]
[Route("api/guidelines")]
[Authorize(Roles = "manager")]
public class GuidelinesController : ControllerBase
{
    private readonly GuidelineService _guidelineService;

    public GuidelinesController(GuidelineService guidelineService)
    {
        _guidelineService = guidelineService;
    }

    /// <summary>
    /// Get all guidelines
    /// </summary>
    /// <response code="200">List of guidelines</response>
    /// <response code="401">Unauthorized</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<GuidelineResponse>>> GetAllGuidelines()
    {
        var guidelines = await _guidelineService.GetAllGuidelines();
        
        if (guidelines.Count == 0)
        {
            return Ok(new { message = "No guidelines found", data = guidelines });
        }
        
        return Ok(new { message = "Guidelines retrieved successfully", count = guidelines.Count, data = guidelines });
    }

    /// <summary>
    /// Get guideline by ID
    /// </summary>
    /// <param name="id">Guideline ID</param>
    /// <response code="200">Guideline details</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="404">Guideline not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GuidelineResponse>> GetGuidelineById(Guid id)
    {
        var guideline = await _guidelineService.GetGuidelineById(id);
        if (guideline == null)
            return NotFound(new { message = "Guideline not found" });

        return Ok(new { message = "Guideline retrieved successfully", data = guideline });
    }

    /// <summary>
    /// Create new guideline
    /// </summary>
    /// <param name="request">Guideline details</param>
    /// <response code="201">Guideline created successfully</response>
    /// <response code="400">Invalid input data</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden - Admin or Manager only</response>
    [HttpPost]
    [Authorize(Roles = "admin,manager")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<GuidelineResponse>> CreateGuideline([FromBody] CreateGuidelineRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { message = "Invalid input data", errors = ModelState });

        var guideline = await _guidelineService.CreateGuideline(request);
        return CreatedAtAction(nameof(GetGuidelineById), new { id = guideline.GuidelineId }, new
        {
            message = "Guideline created successfully",
            data = guideline
        });
    }

    /// <summary>
    /// Update guideline
    /// </summary>
    /// <param name="id">Guideline ID</param>
    /// <param name="request">Updated guideline details</param>
    /// <response code="200">Guideline updated successfully</response>
    /// <response code="400">Invalid input data</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden - Admin or Manager only</response>
    /// <response code="404">Guideline not found</response>
    [HttpPut("{id}")]
    [Authorize(Roles = "admin,manager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GuidelineResponse>> UpdateGuideline(Guid id, [FromBody] UpdateGuidelineRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { message = "Invalid input data", errors = ModelState });

        var guideline = await _guidelineService.UpdateGuideline(id, request);
        if (guideline == null)
            return NotFound(new { message = "Guideline not found" });

        return Ok(new
        {
            message = "Guideline updated successfully",
            data = guideline
        });
    }

    /// <summary>
    /// Delete guideline
    /// </summary>
    /// <param name="id">Guideline ID</param>
    /// <response code="200">Guideline deleted successfully</response>
    /// <response code="400">Cannot delete guideline in use</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden - Admin or Manager only</response>
    /// <response code="404">Guideline not found</response>
    [HttpDelete("{id}")]
    [Authorize(Roles = "admin,manager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteGuideline(Guid id)
    {
        var (success, message) = await _guidelineService.DeleteGuideline(id);
        
        if (!success)
            return BadRequest(new { message });

        return Ok(new { message });
    }
}
