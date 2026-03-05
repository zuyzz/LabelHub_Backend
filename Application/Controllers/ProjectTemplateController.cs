using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DataLabelProject.Business.Services.Projects;
using DataLabelProject.Application.DTOs.Projects;

namespace DataLabelProject.Application.Controllers
{
    [ApiController]
    [Route("api/project-templates")]
    public class ProjectTemplateController : ControllerBase
    {
        private readonly IProjectTemplateService _service;

        public ProjectTemplateController(IProjectTemplateService service)
        {
            _service = service;
        }

        /// <summary>
        /// Get all project templates
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllTemplates()
        {
            var templates = await _service.GetAllAsync();
            return Ok(templates);
        }

        /// <summary>
        /// Get project template by id
        /// </summary>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetTemplate(Guid id)
        {
            var template = await _service.GetByIdAsync(id);
            if (template == null)
                return NotFound();
            return Ok(template);
        }

        /// <summary>
        /// Create a new project template (admin only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> CreateTemplate([FromBody] ProjectTemplateCreateRequest dto)
        {
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetTemplate), new { id = created.TemplateId }, created);
        }

        /// <summary>
        /// Update project template (admin only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateTemplate(Guid id, [FromBody] ProjectTemplateUpdateRequest dto)
        {
            var updated = await _service.UpdateAsync(id, dto);
            if (updated == null)
                return NotFound();
            return Ok(updated);
        }

        /// <summary>
        /// Delete project template (admin only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteTemplate(Guid id)
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted)
                return NotFound();
            return NoContent();
        }
    }
}
