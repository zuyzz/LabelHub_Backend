using DataLabelProject.Application.DTOs.Exports;
using DataLabelProject.Business.Services.Exports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DataLabelProject.Application.Controllers;

[ApiController]
[Route("api/exports")]
public class ExportsController : ControllerBase
{
    private readonly IExportService _exportService;

    public ExportsController(IExportService exportService)
    {
        _exportService = exportService;
    }

    [HttpGet]
    [Authorize(Roles = "manager")]
    public async Task<IActionResult> GetExports()
    {
        var exports = await _exportService.GetExports();
        return Ok(exports);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "manager")]
    public async Task<IActionResult> GetExportById(Guid id)
    {
        var export = await _exportService.GetExportById(id);
        if (export == null)
            return NotFound();

        return Ok(export);
    }

    [HttpPost("{projectId}")]
    [Authorize(Roles = "manager")]
    public async Task<IActionResult> CreateExport(Guid projectId, [FromBody] CreateExportRequest request)
    {
        var export = await _exportService.CreateExport(projectId, request);
        return CreatedAtAction(nameof(GetExportById), new { id = export.ExportId }, export);
    }
}
