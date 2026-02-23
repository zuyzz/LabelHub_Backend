using DataLabelProject.Application.DTOs;
using DataLabelProject.Application.DTOs.Labels;
using DataLabelProject.Business.Services.Labels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DataLabelProject.Application.Controllers;

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
