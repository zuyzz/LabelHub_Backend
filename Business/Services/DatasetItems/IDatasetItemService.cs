using DataLabelProject.Application.DTOs.Common;
using DataLabelProject.Application.DTOs.Datasets;

namespace DataLabelProject.Business.Services.DatasetItems
{
    public interface IDatasetItemService
    {
        Task<PagedResponse<DatasetItemResponse>> GetDataItemsByDatasetId(Guid datasetId, DatasetItemQueryParameters @params);
        Task<DatasetItemResponse?> GetDataItemById(Guid id);
        Task<DatasetItemResponse> CreateDataItem(Guid datasetId, string mediaType, string storageUri, string metadata);
        Task<bool> DeleteDataItem(Guid id);
    }
}
