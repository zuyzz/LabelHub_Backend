using DataLabelProject.Business.Models;

namespace DataLabelProject.Data.Repositories.Abstractions;

public interface IDatasetRepository
{
    Task<Dataset> CreateDatasetAsync(Dataset dataset);
    Task<Dataset?> GetDatasetByIdAsync(Guid datasetId);
    Task<IEnumerable<Dataset>> GetAllDatasetsAsync();
    Task<IEnumerable<Dataset>> GetDatasetsByCreatorAsync(Guid creatorId);
    Task<Dataset?> GetByNameAndCreatorAsync(string name, Guid creatorId);
    Task<Dataset> UpdateDatasetAsync(Dataset dataset);
    Task DeleteDatasetAsync(Guid datasetId);
    Task SaveChangesAsync();
}

