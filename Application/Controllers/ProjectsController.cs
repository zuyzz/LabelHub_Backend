using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DataLabelProject.Business.Services.Projects;
using DataLabelProject.Business.Services.Guidelines;
using DataLabelProject.Application.DTOs;
using DataLabelProject.Application.DTOs.Projects;
using DataLabelProject.Application.DTOs.Common;

namespace DataLabelProject.Application.Controllers
{
    [ApiController]
    [Route("api/projects")]
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectService _service;
    private readonly IProjectDatasetService _projectDatasetService;
    private readonly IGuidelineService _guidelineService;

    public ProjectsController(
        IProjectService service,
        DataLabelProject.Business.Services.Projects.IProjectDatasetService projectDatasetService,
        DataLabelProject.Business.Services.Guidelines.IGuidelineService guidelineService)
    {
        _service = service;
        _projectDatasetService = projectDatasetService;
        _guidelineService = guidelineService;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetProjects([FromQuery] ProjectQueryParameters query)
    {
        var projects = await _service.GetProjectsAsync(query ?? new ProjectQueryParameters());
        return Ok(projects);
    }

        /// <summary>
        /// Get all active members of a project
        /// </summary>
        [HttpGet("{id}/members")]
        [Authorize]
        public async Task<IActionResult> GetProjectMembers(Guid id)
        {
            // Verify that the project exists
            var project = await _service.GetByIdAsync(id);
            if (project == null) return NotFound();

            var members = await _service.GetProjectMembersAsync(id);
            return Ok(members);
        }

        /// <summary>
        /// Join the project by id for the current authenticated user
        /// </summary>
        [HttpPost("{id}/join")]
        [Authorize]
        public async Task<IActionResult> JoinProject(Guid id)
        {
            var result = await _service.JoinProjectAsync(id);
            return result switch
            {
                JoinProjectResult.ProjectNotFound => NotFound(),
                JoinProjectResult.Unauthorized => Unauthorized(),
                JoinProjectResult.Forbidden => Forbid(),
                JoinProjectResult.AlreadyMember => Conflict(new { message = "Already a member" }),
                JoinProjectResult.Success => NoContent(),
                _ => StatusCode(500)
            };
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProject(Guid id)
        {
            var project = await _service.GetByIdAsync(id);
            if (project == null) return NotFound();
            return Ok(project);
        }

        /// <summary>
        /// Get all datasets attached to a project
        /// </summary>
        [HttpGet("{id}/datasets")]
        public async Task<IActionResult> GetProjectDatasets(Guid id)
        {
            var project = await _service.GetByIdAsync(id);
            if (project == null) return NotFound();

            var datasets = await _projectDatasetService.GetDatasetsByProjectAsync(id);
            return Ok(datasets);
        }

        /// <summary>
        /// Get the guideline associated with a project
        /// </summary>
        [HttpGet("{id}/guideline")]
        public async Task<IActionResult> GetProjectGuideline(Guid id)
        {
            var project = await _service.GetByIdAsync(id);
            if (project == null) return NotFound();

            var guideline = await _guidelineService.GetGuidelineByProjectAsync(id);
            if (guideline == null) return NotFound();
            return Ok(guideline);
        }

        [HttpPost]
        [Authorize(Roles = "admin,manager")]
        public async Task<IActionResult> CreateProject([FromBody] ProjectCreateRequest dto)
        {
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetProject), new { id = created.ProjectId }, created);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "admin,manager")]
        public async Task<IActionResult> UpdateProject(Guid id, [FromBody] ProjectUpdateRequest dto)
        {
            var updated = await _service.UpdateAsync(id, dto);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(Guid id)
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}
