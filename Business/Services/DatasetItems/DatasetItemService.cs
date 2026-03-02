using DataLabelProject.Application.DTOs.Datasets;
using DataLabelProject.Business.Models;
using DataLabelProject.Data.Repositories.Abstractions;

namespace DataLabelProject.Business.Services.DatasetItems;

public class DatasetItemService : IDatasetItemService
{
    private readonly IDatasetItemRepository _repo;
    private readonly Storage.IFileStorage _storage;

    public DatasetItemService(IDatasetItemRepository repo, Storage.IFileStorage storage)
    {
        _repo = repo;
        _storage = storage;
    }

    public async Task<IEnumerable<DatasetItemResponse>> GetDatasetItemsAsync(Guid datasetId)
    {
        var items = await _repo.GetDatasetItemsAsync(datasetId);
        return items.Select(item => new DatasetItemResponse(
            item.ItemId,
            item.DatasetId,
            item.MediaType,
            item.StorageUri,
            item.Metadata,
            item.CreatedAt));
    }

    public async Task<DatasetItemResponse> GetDatasetItemByIdAsync(Guid itemId)
    {
        var item = await _repo.GetDatasetItemByIdAsync(itemId);
        if (item == null)
            throw new KeyNotFoundException($"Dataset item with ID {itemId} not found");

        return new DatasetItemResponse(
            item.ItemId,
            item.DatasetId,
            item.MediaType,
            item.StorageUri,
            item.Metadata,
            item.CreatedAt);
    }

    public async Task<DatasetItemResponse> CreateDatasetItemAsync(Guid datasetId, string mediaType, string storageUri, string? metadata = null)
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

        await _repo.CreateDatasetItemAsync(item);
        await _repo.SaveChangesAsync();

        return new DatasetItemResponse(
            item.ItemId,
            item.DatasetId,
            item.MediaType,
            item.StorageUri,
            item.Metadata,
            item.CreatedAt);
    }

    public async Task DeleteDatasetItemAsync(Guid itemId)
    {
        var item = await _repo.GetDatasetItemByIdAsync(itemId);
        if (item == null) return;

        // delete storage object if exists
        if (!string.IsNullOrWhiteSpace(item.StorageUri))
        {
            try
            {
                await _storage.DeleteFileAsync(item.StorageUri);
            }
            catch
            {
                // swallow storage delete errors but still remove DB record
            }
        }

        await _repo.DeleteDatasetItemAsync(itemId);
        await _repo.SaveChangesAsync();
    }
}
