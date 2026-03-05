using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DataLabelProject.Business.Services.Export;

namespace DataLabelProject.Application.Controllers;

[ApiController]
[Route("api/exports")]
[Authorize(Roles = "admin,manager")]
public class ExportController : ControllerBase
{
    private readonly IExportService _exportService;

    public ExportController(IExportService exportService)
    {
        _exportService = exportService;
    }

    /// <summary>
    /// Export a dataset as COCO JSON format.
    /// Only managers and admins can export datasets.
    /// </summary>
    /// <param name="datasetId">The dataset ID to export</param>
    /// <returns>COCO JSON file</returns>
    [HttpGet("datasets/{datasetId:guid}/coco")]
    public async Task<IActionResult> ExportDatasetAsCoco([FromRoute] Guid datasetId)
    {
        try
        {
            var stream = await _exportService.ExportAsCocoJsonAsync(datasetId);
            var filename = $"dataset-{datasetId:N}-coco.json";
            
            return File(stream, "application/json", filename);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "Error exporting dataset", error = ex.Message });
        }
    }
}
