using DataLabelProject.Application.DTOs;
using DataLabelProject.Application.DTOs.Projects;
using DataLabelProject.Business.Services.Projects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DataLabelProject.Application.Controllers
{
    [ApiController]
    [Route("api/projects/{projectId}/versions")]
    [Authorize]
    public class ProjectVersionController : ControllerBase
    {
        private readonly IProjectVersionService _projectVersionService;

        public ProjectVersionController(IProjectVersionService projectVersionService)
        {
            _projectVersionService = projectVersionService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromRoute] Guid projectId)
        {
            var versions = await _projectVersionService.GetAllByProjectAsync(projectId);
            return Ok(versions);
        }

        [HttpGet("draft")]
        public async Task<IActionResult> GetDraft([FromRoute] Guid projectId)
        {
            var draft = await _projectVersionService.GetDraftAsync(projectId);
            return Ok(draft);
        }

        [HttpGet("latest")]
        public async Task<IActionResult> GetLatestReleased([FromRoute] Guid projectId)
        {
            var version = await _projectVersionService.GetLatestReleasedAsync(projectId);
            return Ok(version);
        }

        [HttpPost]
        public async Task<IActionResult> CreateDraft(
            [FromRoute] Guid projectId,
            [FromBody] CreateProjectVersionRequest request
        )
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var pv = await _projectVersionService.CreateDraftAsync(
                projectId,
                request.DatasetId,
                request.LabelSetId,
                request.GuidelineId
            );

            return CreatedAtAction(nameof(GetAll), new { projectId }, pv);
        }

        [HttpPost("{projectVersionId}/release")]
        public async Task<IActionResult> Release(
            [FromRoute] Guid projectVersionId
        )
        {
            await _projectVersionService.ReleaseAsync(projectVersionId);
            return NoContent();
        }
    }
}
