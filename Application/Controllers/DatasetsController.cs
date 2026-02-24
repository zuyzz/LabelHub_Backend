using DataLabelProject.Application.DTOs.Datasets;
using DataLabelProject.Business.Services.Datasets;
using DataLabelProject.Business.Services.DatasetItems;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DataLabelProject.Application.Controllers
{
    [ApiController]
    [Route("api/dataset")]
    public class DatasetsController : ControllerBase
    {
        private readonly IDatasetService _datasetService;
        private readonly IDatasetItemService _itemService;

        public DatasetsController(IDatasetService datasetService, IDatasetItemService itemService)
        {
            _datasetService = datasetService;
            _itemService = itemService;
        }

        /// <summary>
        /// Create a new dataset. File upload is optional.
        /// Accepts image files or archive files (zip/rar) containing only image files.
        /// </summary>
        [HttpPost]
        [Consumes("multipart/form-data")]
        [Authorize]
        public async Task<IActionResult> CreateDataset([FromForm] CreateDatasetRequest request)
        {
            var result = await _datasetService.CreateDatasetAsync(request);
            return Ok(result);
        }

        /// <summary>
        /// Get dataset by ID with item count and metadata.
        /// </summary>
        [HttpGet("{datasetId:guid}")]
        [Authorize]
        public async Task<IActionResult> GetDataset([FromRoute] Guid datasetId)
        {
            var result = await _datasetService.GetDatasetByIdAsync(datasetId);
            return Ok(result);
        }

        /// <summary>
        /// Get all dataset items for a specific dataset.
        /// </summary>
        [HttpGet("{datasetId:guid}/items")]
        [Authorize]
        public async Task<IActionResult> GetDatasetItems([FromRoute] Guid datasetId)
        {
            var result = await _itemService.GetDatasetItemsAsync(datasetId);
            return Ok(result);
        }

        /// <summary>
        /// Get a single dataset item by ID.
        /// </summary>
        [HttpGet("item/{itemId:guid}")]
        [Authorize]
        public async Task<IActionResult> GetDatasetItem([FromRoute] Guid itemId)
        {
            var result = await _itemService.GetDatasetItemByIdAsync(itemId);
            return Ok(result);
        }

        /// <summary>
        /// Create a new dataset item manually (e.g., for programmatic uploads or batch operations).
        /// </summary>
        [HttpPost("item")]
        [Authorize]
        public async Task<IActionResult> CreateDatasetItem([FromBody] CreateDatasetItemRequest request)
        {
            var result = await _itemService.CreateDatasetItemAsync(request.DatasetId, request.MediaType, request.StorageUri);
            return Ok(result);
        }

        /// <summary>
        /// Delete a dataset item by ID.
        /// </summary>
        [HttpDelete("item/{itemId:guid}")]
        [Authorize]
        public async Task<IActionResult> DeleteDatasetItem([FromRoute] Guid itemId)
        {
            await _itemService.DeleteDatasetItemAsync(itemId);
            return NoContent();
        }

        /// <summary>
        /// Update an existing dataset. Can update name and description.
        /// </summary>
        [HttpPut("{datasetId:guid}")]
        [Authorize]
        public async Task<IActionResult> UpdateDataset([FromRoute] Guid datasetId, [FromBody] UpdateDatasetRequest request)
        {
            var result = await _datasetService.UpdateDatasetAsync(datasetId, request);
            return Ok(result);
        }

        /// <summary>
        /// Delete a dataset and all its associated data.
        /// </summary>
        [HttpDelete("{datasetId:guid}")]
        [Authorize]
        public async Task<IActionResult> DeleteDataset([FromRoute] Guid datasetId)
        {
            await _datasetService.DeleteDatasetAsync(datasetId);
            return NoContent();
        }
    }
}
