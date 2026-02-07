using DataLabel_Project_BE.DTOs;
using DataLabel_Project_BE.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DataLabel_Project_BE.Controllers
{
    [ApiController]
    [Route("api/projects/{projectId}/versions")]
    [Authorize]
    public class ProjectVersionsController : ControllerBase
    {
        private readonly IProjectVersionService _projectVersionService;

        public ProjectVersionsController(IProjectVersionService projectVersionService)
        {
            _projectVersionService = projectVersionService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(Guid projectId)
        {
            var versions = await _projectVersionService.GetAllByProjectAsync(projectId);
            return Ok(versions);
        }

        [HttpGet("draft")]
        public async Task<IActionResult> GetDraft(Guid projectId)
        {
            var draft = await _projectVersionService.GetDraftAsync(projectId);
            return Ok(draft);
        }

        [HttpGet("latest")]
        public async Task<IActionResult> GetLatestReleased(Guid projectId)
        {
            var version = await _projectVersionService.GetLatestReleasedAsync(projectId);
            return Ok(version);
        }

        [HttpPost]
        public async Task<IActionResult> CreateDraft(
            Guid projectId,
            [FromBody] CreateProjectVersionRequest request
        )
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var pv = await _projectVersionService.CreateDraftAsync(
                projectId,
                request.DatasetId,
                request.LabelSetId,
                request.GuidelineId,
                userId
            );

            return CreatedAtAction(nameof(GetAll), new { projectId }, pv);
        }

        [HttpPost("{projectVersionId}/release")]
        public async Task<IActionResult> Release(
            Guid projectVersionId
        )
        {
            await _projectVersionService.ReleaseAsync(projectVersionId);
            return NoContent();
        }
    }
}
