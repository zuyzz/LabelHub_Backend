using DataLabelProject.Application.DTOs.Datasets;
using DataLabelProject.Business.Models;

namespace DataLabelProject.Data.Repositories.Abstractions;

public interface IDatasetItemRepository
{
    Task<(IEnumerable<DatasetItem> Items, int TotalCount)> GetAllByDatasetIdAsync(Guid datasetId, DatasetItemQueryParameters @params);
    Task<IEnumerable<DatasetItem>> GetAllByDatasetIdAsync(Guid datasetId);
    Task<DatasetItem?> GetByIdAsync(Guid id);
    Task CreateAsync(DatasetItem item);
    Task DeleteAsync(DatasetItem item);
    Task SaveChangesAsync();
}
