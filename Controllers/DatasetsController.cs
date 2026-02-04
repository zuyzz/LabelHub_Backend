using DataLabel_Project_BE.DTOs;
using DataLabel_Project_BE.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DataLabel_Project_BE.Controllers
{
    [ApiController]
    [Route("api/projects/{projectId:guid}/datasets")]
    public class DatasetsController : ControllerBase
    {
        private readonly IDatasetService _service;
        private readonly ILogger<DatasetsController> _logger;

        public DatasetsController(IDatasetService service, ILogger<DatasetsController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Import dataset into a project. Accepts single image file or text file, or an archive (zip/rar) containing only images or only text files.
        /// </summary>
        [HttpPost("import")]
        [Consumes("multipart/form-data")]
        [Authorize]
        public async Task<IActionResult> ImportDataset(
            [FromRoute] Guid projectId,
            [FromForm] DatasetImportRequest request)
        {
            _logger.LogCritical("IMPORT: Controller entered");
            if (request.File == null)
                return BadRequest("File is required");

            var result = await _service.ImportDatasetAsync(projectId, request);
            return Ok(result);
        }

        private static string GetContentTypeByExtension(string? ext)
        {
            if (string.IsNullOrWhiteSpace(ext)) return "application/octet-stream";
            return ext.ToLowerInvariant() switch
            {
                ".png" => "image/png",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                ".json" => "application/json",
                ".xml" => "application/xml",
                ".zip" => "application/zip",
                ".rar" => "application/vnd.rar",
                _ => "application/octet-stream",
            };
        }
    }
}