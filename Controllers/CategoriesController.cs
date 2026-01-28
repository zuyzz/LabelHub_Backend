using Microsoft.AspNetCore.Mvc;

namespace DataLabel_Project_BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        // Mock endpoint - Get all categories
        [HttpGet]
        public IActionResult GetCategories()
        {
            // TODO: Implement real category management
            return Ok(new[]
            {
                new { id = Guid.NewGuid(), name = "Image Classification (mock)" },
                new { id = Guid.NewGuid(), name = "Object Detection (mock)" },
                new { id = Guid.NewGuid(), name = "Semantic Segmentation (mock)" }
            });
        }

        // Mock endpoint - Create category
        [HttpPost]
        public IActionResult CreateCategory([FromBody] CreateCategoryRequest request)
        {
            // TODO: Implement real category creation
            return Ok(new
            {
                id = Guid.NewGuid(),
                name = request.Name,
                message = "Category created successfully (mock)"
            });
        }
    }

    public class CreateCategoryRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
