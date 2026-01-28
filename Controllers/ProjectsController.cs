using Microsoft.AspNetCore.Mvc;

namespace DataLabel_Project_BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectsController : ControllerBase
    {
        // Mock endpoint - Get all projects
        [HttpGet]
        public IActionResult GetProjects()
        {
            // TODO: Implement real project management
            return Ok(new[]
            {
                new { id = Guid.NewGuid(), name = "Project 1 (mock)", status = "Active" },
                new { id = Guid.NewGuid(), name = "Project 2 (mock)", status = "Completed" }
            });
        }

        // Mock endpoint - Create project
        [HttpPost]
        public IActionResult CreateProject([FromBody] CreateProjectRequest request)
        {
            // TODO: Implement real project creation
            return Ok(new
            {
                id = Guid.NewGuid(),
                name = request.Name,
                message = "Project created successfully (mock)"
            });
        }
    }

    public class CreateProjectRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
