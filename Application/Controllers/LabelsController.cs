using DataLabelProject.Business.Services.Labels;
using DataLabelProject.Application.DTOs.Labels;
using Microsoft.AspNetCore.Mvc;

namespace DataLabelProject.Application.Controllers;

[ApiController]
[Route("api/labels")]
public class LabelsController : ControllerBase
{
    private readonly ILabelService _labelService;

    public LabelsController(ILabelService labelService)
    {
        _labelService = labelService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var labels = await _labelService.GetAllLabelsAsync();
        return Ok(labels);
    }

    [HttpGet("categories/{categoryId}/labels")]
    public async Task<IActionResult> GetByCategory(Guid categoryId)
    {
        var labels = await _labelService.GetLabelsByCategoryAsync(categoryId);
        return Ok(labels);
    }

    [HttpPost("labels")]
    public async Task<IActionResult> CreateLabel(
        [FromBody] CreateLabelRequest request)
    {
        var label = await _labelService.CreateLabelAsync(request.CategoryId, request.Name, request.CreatedBy);
            return Ok(label);
    }

    [HttpPut("labels/{labelId}")]
    public async Task<IActionResult> UpdateLabel(
        Guid labelId,
        [FromBody] UpdateLabelRequest request)
    {
        await _labelService.UpdateLabelAsync(labelId, request.Name, request.IsActive);
        return NoContent();
    }

    [HttpDelete("labels/{labelId}")]
    public async Task<IActionResult> DeleteLabel(Guid labelId)
    {
        await _labelService.DeleteLabelAsync(labelId);
        return NoContent();
    }
}

