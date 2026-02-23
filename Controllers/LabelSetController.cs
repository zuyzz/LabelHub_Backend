using DataLabel_Project_BE.DTOs;
using DataLabel_Project_BE.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DataLabel_Project_BE.Controllers;

[ApiController]
[Route("api/projects/{projectId:guid}/label-sets")]
public class LabelSetController : ControllerBase
{
    private readonly ILabelSetService _service;

    public LabelSetController(ILabelSetService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    [HttpPost]
    [Authorize(Roles = "manager")]
    public async Task<IActionResult> Create(Guid projectId, CreateLabelSetRequest request)
    {
        var result = await _service.CreateAsync(projectId, request, null);
        return Ok(result);
    }
}
