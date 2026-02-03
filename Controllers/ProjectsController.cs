using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DataLabel_Project_BE.Services;
using DataLabel_Project_BE.DTOs;

namespace DataLabel_Project_BE.Controllers
{
    [ApiController]
    [Route("api/projects")]
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectService _service;

        public ProjectsController(IProjectService service)
        {
            _service = service;
        }

        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetProjects([FromQuery] DTOs.ProjectQueryParameters query)
        {
            var projects = await _service.GetProjectsAsync(query ?? new DTOs.ProjectQueryParameters());
            return Ok(projects);
        }

        /// <summary>
        /// Get projects that current authenticated user has joined
        /// </summary>
        [HttpGet("mine")]
        [Authorize]
        public async Task<IActionResult> GetMyProjects([FromQuery] DTOs.ProjectQueryParameters query)
        {
            var projects = await _service.GetUserProjectsAsync(query ?? new DTOs.ProjectQueryParameters());
            return Ok(projects);
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
                DTOs.JoinProjectResult.ProjectNotFound => NotFound(),
                DTOs.JoinProjectResult.Unauthorized => Unauthorized(),
                DTOs.JoinProjectResult.Forbidden => Forbid(),
                DTOs.JoinProjectResult.AlreadyMember => Conflict(new { message = "Already a member" }),
                DTOs.JoinProjectResult.Success => NoContent(),
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
