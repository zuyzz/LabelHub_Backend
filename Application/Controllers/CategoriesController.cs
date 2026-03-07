using DataLabelProject.Application.DTOs.Categories;
using DataLabelProject.Business.Services.Categories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace DataLabelProject.Application.Controllers
{
    [ApiController]
    [Route("api/categories")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _categoryService.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _categoryService.GetByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request)
        {
            // Get current user ID from claims
            var userIdClaim = User.FindFirst("userId")?.Value;
            Guid? createdBy = null;
            if (Guid.TryParse(userIdClaim, out var userId))
            {
                createdBy = userId;
            }

            var result = await _categoryService.CreateAsync(request, createdBy);
            return CreatedAtAction(nameof(GetById), new { id = result.CategoryId }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryRequest request)
        {
            var success = await _categoryService.UpdateAsync(id, request);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _categoryService.DeleteAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}
