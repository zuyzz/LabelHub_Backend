using DataLabelProject.Application.DTOs.Categories;
using DataLabelProject.Business.Services.Categories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DataLabelProject.Application.Controllers;

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
    [Authorize(Roles = "admin,manager")]
    public async Task<IActionResult> GetAll([FromQuery] CategoryQueryParameters @params) 
    {
        var result = await _categoryService.GetCategories(@params);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "admin,manager")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _categoryService.GetCategoryById(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "admin,manager")]
    public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request)
    {
        var result = await _categoryService.CreateCategory(request);
        return CreatedAtAction(nameof(GetById), new { id = result.CategoryId }, result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "admin,manager")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryRequest request)
    {
        var result = await _categoryService.UpdateCategory(id, request);
        if (result == null) return NotFound();
        return Ok(result);
    }
}
