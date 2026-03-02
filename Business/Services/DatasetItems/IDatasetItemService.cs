using DataLabelProject.Application.DTOs.Datasets;
using DataLabelProject.Business.Models;

namespace DataLabelProject.Business.Services.DatasetItems;

public interface IDatasetItemService
{
    Task<IEnumerable<DatasetItemResponse>> GetDatasetItemsAsync(Guid datasetId);
    Task<DatasetItemResponse> GetDatasetItemByIdAsync(Guid itemId);
    Task<DatasetItemResponse> CreateDatasetItemAsync(Guid datasetId, string mediaType, string storageUri, string? metadata = null);
    Task DeleteDatasetItemAsync(Guid itemId);
}
