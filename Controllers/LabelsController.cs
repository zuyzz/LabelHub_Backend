using DataLabel_Project_BE.Services;
using Microsoft.AspNetCore.Mvc;

namespace DataLabel_Project_BE.Controllers
{
    [ApiController]
    [Route("api")]
    public class LabelsController : ControllerBase
    {
        private readonly ILabelService _labelService;

        public LabelsController(ILabelService labelService)
        {
            _labelService = labelService;
        }

        [HttpGet("labelsets/{labelSetId}/labels")]
        public async Task<IActionResult> GetLabels(Guid labelSetId)
        {
            var labels = await _labelService.GetLabelsByLabelSetAsync(labelSetId);
            return Ok(labels);
        }

        [HttpPost("labelsets/{labelSetId}/labels")]
        public async Task<IActionResult> CreateLabel(
            Guid labelSetId,
            [FromBody] CreateLabelRequest request)
        {
            var label = await _labelService.CreateLabelAsync(labelSetId, request.Name);
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
}

