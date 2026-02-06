using DataLabel_Project_BE.DTOs;
using DataLabel_Project_BE.Services;
using Microsoft.AspNetCore.Mvc;

namespace DataLabel_Project_BE.Controllers;

[ApiController]
[Route("api/label-sets")]
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
    public async Task<IActionResult> Create(CreateLabelSetRequest request)
    {
        var result = await _service.CreateAsync(request, null);
        return Ok(result);
    }

    [HttpPost("{labelSetId}/versions")]
    public async Task<IActionResult> CreateNewVersion(
        Guid labelSetId,
        UpdateLabelSetRequest request)
    {
        var result = await _service.CreateNewVersionAsync(labelSetId, request, null);
        if (result == null) return NotFound();

        return Ok(result);
    }
}
