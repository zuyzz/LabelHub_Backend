using DataLabelProject.Application.DTOs.Datasets;
using DataLabelProject.Business.Models;
using DataLabelProject.Data.Repositories.Abstractions;

namespace DataLabelProject.Business.Services.DatasetItems;

public class DatasetItemService : IDatasetItemService
{
    private readonly IDatasetItemRepository _repo;

    public DatasetItemService(IDatasetItemRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<DatasetItemResponse>> GetDatasetItemsAsync(Guid datasetId)
    {
        var items = await _repo.GetDatasetItemsAsync(datasetId);
        return items.Select(item => new DatasetItemResponse(
            item.ItemId,
            item.DatasetId,
            item.MediaType,
            item.StorageUri,
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
            item.CreatedAt);
    }

    public async Task<DatasetItemResponse> CreateDatasetItemAsync(Guid datasetId, string mediaType, string storageUri)
    {
        var item = new DatasetItem
        {
            ItemId = Guid.NewGuid(),
            DatasetId = datasetId,
            MediaType = mediaType,
            StorageUri = storageUri,
            CreatedAt = DateTime.UtcNow
        };

        await _repo.CreateDatasetItemAsync(item);
        await _repo.SaveChangesAsync();

        return new DatasetItemResponse(
            item.ItemId,
            item.DatasetId,
            item.MediaType,
            item.StorageUri,
            item.CreatedAt);
    }

    public async Task DeleteDatasetItemAsync(Guid itemId)
    {
        await _repo.DeleteDatasetItemAsync(itemId);
        await _repo.SaveChangesAsync();
    }
}
