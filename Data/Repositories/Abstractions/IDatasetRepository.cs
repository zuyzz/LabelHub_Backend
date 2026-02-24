using DataLabelProject.Business.Models;

namespace DataLabelProject.Data.Repositories.Abstractions;

public interface IDatasetRepository
{
    Task<Dataset> CreateDatasetAsync(Dataset dataset);
    Task<Dataset?> GetDatasetByIdAsync(Guid datasetId);
    Task<Dataset> UpdateDatasetAsync(Dataset dataset);
    Task DeleteDatasetAsync(Guid datasetId);
    Task SaveChangesAsync();
}

