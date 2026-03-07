using DataLabelProject.Business.Services.Labels;
using DataLabelProject.Application.DTOs.Labels;
using Microsoft.AspNetCore.Mvc;

namespace DataLabelProject.Application.Controllers
{
    [ApiController]
    [Route("api/projects/{projectId:guid}/labelsets/{labelSetId:guid}/labels")]
    public class LabelsController : ControllerBase
    {
        private readonly ILabelService _labelService;

        public LabelsController(ILabelService labelService)
        {
            _labelService = labelService;
        }

        [HttpGet]
        public async Task<IActionResult> GetLabels(Guid projectId, Guid labelSetId)
        {
            // Check if labelSet belongs to project
            var labelSet = await _labelService.GetLabelSetByIdAsync(labelSetId);
            if (labelSet == null || labelSet.ProjectId != projectId) return NotFound();

            var labels = await _labelService.GetLabelsByLabelSetAsync(labelSetId);
            return Ok(labels);
        }

        [HttpPost]
        public async Task<IActionResult> CreateLabel(
            Guid projectId,
            Guid labelSetId,
            [FromBody] CreateLabelRequest request)
        {
            // Check if labelSet belongs to project
            var labelSet = await _labelService.GetLabelSetByIdAsync(labelSetId);
            if (labelSet == null || labelSet.ProjectId != projectId) return NotFound();

            var label = await _labelService.CreateLabelAsync(labelSetId, request.Name);
            return Ok(label);
        }

        [HttpPut("{labelId:guid}")]
        public async Task<IActionResult> UpdateLabel(
            Guid projectId,
            Guid labelSetId,
            Guid labelId,
            [FromBody] UpdateLabelRequest request)
        {
            // Check if labelSet belongs to project
            var labelSet = await _labelService.GetLabelSetByIdAsync(labelSetId);
            if (labelSet == null || labelSet.ProjectId != projectId) return NotFound();

            await _labelService.UpdateLabelAsync(labelId, request.Name, request.IsActive);
            return NoContent();
        }

        [HttpDelete("{labelId:guid}")]
        public async Task<IActionResult> DeleteLabel(
            Guid projectId,
            Guid labelSetId,
            Guid labelId)
        {
            // Check if labelSet belongs to project
            var labelSet = await _labelService.GetLabelSetByIdAsync(labelSetId);
            if (labelSet == null || labelSet.ProjectId != projectId) return NotFound();

            await _labelService.DeleteLabelAsync(labelId);
            return NoContent();
        }
    }
}
