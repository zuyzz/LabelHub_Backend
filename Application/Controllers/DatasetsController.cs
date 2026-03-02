using DataLabelProject.Application.DTOs.Datasets;
using DataLabelProject.Business.Services.Datasets;
using DataLabelProject.Business.Services.DatasetItems;
using DataLabelProject.Business.Services.FileUpload.Metadata;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DataLabelProject.Application.Controllers
{
    [ApiController]
    [Route("api/datasets")]
    [Authorize(Roles = "admin,manager")]
    public class DatasetsController : ControllerBase
    {
        private readonly IDatasetService _datasetService;
        private readonly IDatasetItemService _itemService;
        private readonly IEnumerable<DataLabelProject.Business.Services.FileUpload.IFileUploadStrategy> _strategies;
        private readonly MetadataExtractorFactory _metadataExtractorFactory;

        public DatasetsController(IDatasetService datasetService, IDatasetItemService itemService,
            IEnumerable<DataLabelProject.Business.Services.FileUpload.IFileUploadStrategy> strategies,
            IEnumerable<IMetadataExtractor> metadataExtractors)
        {
            _datasetService = datasetService;
            _itemService = itemService;
            _strategies = strategies;
            _metadataExtractorFactory = new MetadataExtractorFactory(metadataExtractors);
        }

        /// <summary>
        /// Create a new dataset. File upload is optional.
        /// Accepts image files or archive files (zip/rar) containing only image files.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateDataset([FromBody] CreateDatasetRequest request)
        {
            var result = await _datasetService.CreateDatasetAsync(request);
            return Ok(result);
        }

        /// <summary>
        /// Get dataset by ID with item count and metadata.
        /// </summary>
        [HttpGet("{datasetId:guid}")]
        public async Task<IActionResult> GetDataset([FromRoute] Guid datasetId)
        {
            var result = await _datasetService.GetDatasetByIdAsync(datasetId);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetDatasets()
        {
            var result = await _datasetService.GetDatasetsAsync();
            return Ok(result);
        }

        /// <summary>
        /// Get all dataset items for a specific dataset.
        /// </summary>
        [HttpGet("{datasetId:guid}/items")]
        public async Task<IActionResult> GetDatasetItems([FromRoute] Guid datasetId)
        {
            var result = await _itemService.GetDatasetItemsAsync(datasetId);
            return Ok(result);
        }

        /// <summary>
        /// Delete a dataset item by ID.
        /// </summary>
        [HttpDelete("items/{itemId:guid}")]
        public async Task<IActionResult> DeleteDatasetItem([FromRoute] Guid itemId)
        {
            // Service will validate item belongs to dataset and remove storage
            await _itemService.DeleteDatasetItemAsync(itemId);
            return NoContent();
        }

        /// <summary>
        /// Update an existing dataset. Can update name and description.
        /// </summary>
        [HttpPut("{datasetId:guid}")]
        public async Task<IActionResult> UpdateDataset([FromRoute] Guid datasetId, [FromBody] UpdateDatasetRequest request)
        {
            var result = await _datasetService.UpdateDatasetAsync(datasetId, request);
            return Ok(result);
        }

        /// <summary>
        /// Delete a dataset and all its associated data.
        /// </summary>
        [HttpDelete("{datasetId:guid}")]
        public async Task<IActionResult> DeleteDataset([FromRoute] Guid datasetId)
        {
            await _datasetService.DeleteDatasetAsync(datasetId);
            return NoContent();
        }

        /// <summary>
        /// Upload a file or archive into an existing dataset.
        /// Accepts single image/audio/text or archive (zip/rar) containing allowed files.
        /// </summary>
        [HttpPost("{datasetId:guid}/items")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadDatasetItems([FromRoute] Guid datasetId, [FromForm] CreateDatasetItemRequest request)
        {
            var file = request.File;
            if (file == null) return BadRequest(new { message = "File is required" });

            // Validate dataset exists and get name
            var ds = await _datasetService.GetDatasetByIdAsync(datasetId);

            // select a strategy
            var strategy = _strategies.FirstOrDefault(s => s.CanHandle(file));
            if (strategy == null)
                return BadRequest(new { message = "File type not supported" });

            var process = await strategy.ProcessAsync(file, datasetId, ds.Name);

            var created = new List<object>();
            foreach (var item in process.Items)
            {
                // Extract metadata if available
                string? metadata = null;
                var extractor = _metadataExtractorFactory.GetExtractor(item.ContentType);
                if (extractor != null)
                {
                    try
                    {
                        // Create a temporary IFormFile from the StorageUri to extract metadata
                        // Note: In a real scenario, you might want to refactor this to extract metadata
                        // directly from the file before storage
                        metadata = await _metadataExtractorFactory.ExtractMetadataAsync(file);
                    }
                    catch
                    {
                        // Swallow metadata extraction errors - item will be created without metadata
                    }
                }

                var createdItem = await _itemService.CreateDatasetItemAsync(datasetId, item.ContentType, item.StorageUri, metadata);
                created.Add(createdItem);
            }

            return Ok(created);
        }
    }
}
