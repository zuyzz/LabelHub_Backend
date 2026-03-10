using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DataLabelProject.Business.Services.ProjectTemplates;
using DataLabelProject.Application.DTOs.ProjectTemplate;

namespace DataLabelProject.Application.Controllers
{
    [ApiController]
    [Route("api/project-templates")]
    public class ProjectTemplateController : ControllerBase
    {
        private readonly IProjectTemplateService _projectTemplateservice;

        public ProjectTemplateController(IProjectTemplateService projectTemplateService)
        {
            _projectTemplateservice = projectTemplateService;
        }

        /// <summary>
        /// Get all project templates
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllTemplates()
        {
            var templates = await _projectTemplateservice.GetAllAsync();
            return Ok(templates);
        }

        /// <summary>
        /// Get project template by id
        /// </summary>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetTemplate(Guid id)
        {
            var template = await _projectTemplateservice.GetByIdAsync(id);
            if (template == null)
                return NotFound();
            return Ok(template);
        }

        /// <summary>
        /// Create a new project template (admin only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> CreateTemplate([FromBody] CreateProjectTemplateRequest request)
        {
            var created = await _projectTemplateservice.CreateAsync(request);
            return CreatedAtAction(nameof(GetTemplate), new { id = created.TemplateId }, created);
        }

        /// <summary>
        /// Update project template (admin only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateTemplate(Guid id, [FromBody] UpdateProjectTemplateRequest request)
        {
            var updated = await _projectTemplateservice.UpdateAsync(id, request);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        /// <summary>
        /// Delete project template (admin only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteTemplate(Guid id)
        {
            var deleted = await _projectTemplateservice.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}
