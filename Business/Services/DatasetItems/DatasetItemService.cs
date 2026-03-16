using System.Text.Json;
using DataLabelProject.Application.DTOs.Common;
using DataLabelProject.Application.DTOs.Datasets;
using DataLabelProject.Business.Models;
using DataLabelProject.Business.Services.Storage;
using DataLabelProject.Data.Repositories.Abstractions;

namespace DataLabelProject.Business.Services.DatasetItems
{
    public class DatasetItemService : IDatasetItemService
    {
        private readonly IDatasetItemRepository _datasetItemRepository;
        private readonly IDatasetRepository _datasetRepository;
        private readonly ILabelingTaskItemRepository _taskItemRepository;
        private readonly IFileStorage _fileStorage;

        public DatasetItemService(
            IDatasetItemRepository datasetItemRepository, 
            IDatasetRepository datasetRepository,
            ILabelingTaskItemRepository taskItemRepository,
            IFileStorage fileStorage)
        {
            _datasetItemRepository = datasetItemRepository;
            _datasetRepository = datasetRepository;
            _taskItemRepository = taskItemRepository;
            _fileStorage = fileStorage;
        }

        public async Task<PagedResponse<DatasetItemResponse>> GetDataItemsByDatasetId(Guid datasetId, DatasetItemQueryParameters @params)
        {
            var (items, totalCount) = await _datasetItemRepository.GetAllByDatasetIdAsync(datasetId, @params);

            return new PagedResponse<DatasetItemResponse> 
            {
                Items = items.Select(MapToResponse).ToList(),
                TotalItems = totalCount,
                Page = @params.Page,
                PageSize = @params.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)@params.PageSize)
            };
        }

        public async Task<DatasetItemResponse?> GetDataItemById(Guid id)
        {
            var item = await _datasetItemRepository.GetByIdAsync(id);
            if (item == null) return null;

            return MapToResponse(item);
        }

        public async Task<DatasetItemResponse> CreateDataItem(Guid datasetId, string mediaType, string storageUri, string metadata)
        {
            var dataset = await _datasetRepository.GetByIdAsync(datasetId);
            if (dataset == null) 
                throw new InvalidOperationException("Dataset not found");
                
            var jsonMetadata = JsonSerializer.Serialize(metadata);

            var item = new DatasetItem
            {
                DatasetItemId = Guid.NewGuid(),
                DatasetId = datasetId,
                MediaType = mediaType,
                StorageUri = storageUri,
                Metadata = jsonMetadata,
                CreatedAt = DateTime.UtcNow
            };

            await _datasetItemRepository.CreateAsync(item);

            if (dataset.ProjectId.HasValue)
            {
                var taskItem = new LabelingTaskItem
                {
                    TaskItemId = Guid.NewGuid(),
                    TaskId = null,
                    ProjectId = dataset.ProjectId.Value,
                    DatasetItemId = item.DatasetItemId,
                    RevisionCount = 0,
                    Status = Models.Enums.LabelingTaskItemStatus.Unassigned
                };

                await _taskItemRepository.AddAsync(taskItem);
            }

            await _datasetItemRepository.SaveChangesAsync();
            await _taskItemRepository.SaveChangesAsync();

            return MapToResponse(item);
        }

        public async Task<bool> DeleteDataItem(Guid id)
        {
            var item = await _datasetItemRepository.GetByIdAsync(id);
            if (item == null) return false;

            // delete storage object if exists
            if (!string.IsNullOrWhiteSpace(item.StorageUri))
            {
                try
                {
                    await _fileStorage.DeleteFileAsync(item.StorageUri);
                }
                catch
                {
                    throw new InvalidOperationException(
                        $"Failed to delete storage file: {item.StorageUri}");
                }
            }

            await _datasetItemRepository.DeleteAsync(item);
            await _datasetItemRepository.SaveChangesAsync();

            return true;
        }

        private DatasetItemResponse MapToResponse(DatasetItem item)
        {
            ImageMetadata? metadata = null;

            if (!string.IsNullOrWhiteSpace(item.Metadata))
            {
                try
                {
                    metadata = JsonSerializer.Deserialize<ImageMetadata>(item.Metadata);
                }
                catch (JsonException)
                {
                    // log invalid metadata if needed
                }
            }

            return new DatasetItemResponse 
            {
                ItemId = item.DatasetItemId,
                DatasetId = item.DatasetId,
                MediaType = item.MediaType,
                StorageUri = item.StorageUri,
                Metadata = metadata,
                CreatedAt = item.CreatedAt
            };
        }
    }
}
