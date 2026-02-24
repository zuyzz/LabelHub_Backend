using DataLabelProject.Business.Models;

namespace DataLabelProject.Data.Repositories.Abstractions;

public interface IDatasetItemRepository
{
    Task<IEnumerable<DatasetItem>> GetDatasetItemsAsync(Guid datasetId);
    Task<DatasetItem?> GetDatasetItemByIdAsync(Guid itemId);
    Task CreateDatasetItemAsync(DatasetItem item);
    Task DeleteDatasetItemAsync(Guid itemId);
    Task SaveChangesAsync();
}
