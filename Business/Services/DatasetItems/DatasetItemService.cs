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
        private readonly IFileStorage _fileStorage;

        public DatasetItemService(IDatasetItemRepository datasetItemRepository, IFileStorage fileStorage)
        {
            _datasetItemRepository = datasetItemRepository;
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
            var item = new DatasetItem
            {
                ItemId = Guid.NewGuid(),
                DatasetId = datasetId,
                MediaType = mediaType,
                StorageUri = storageUri,
                Metadata = metadata,
                CreatedAt = DateTime.UtcNow
            };

            await _datasetItemRepository.CreateAsync(item);
            await _datasetItemRepository.SaveChangesAsync();

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
            return new DatasetItemResponse 
            {
                ItemId = item.ItemId,
                DatasetId = item.DatasetId,
                MediaType = item.MediaType,
                StorageUri = item.StorageUri,
                Metadata = item.Metadata,
                CreatedAt = item.CreatedAt
            };
        }
    }
}
