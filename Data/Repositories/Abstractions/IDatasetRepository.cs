using DataLabelProject.Application.DTOs.Datasets;
using DataLabelProject.Business.Models;

namespace DataLabelProject.Data.Repositories.Abstractions;

public interface IDatasetRepository
{
    Task<(IEnumerable<Dataset> Items, int TotalCount)> GetAllAsync(DatasetQueryParameters @params);
    Task<(IEnumerable<Dataset> Items, int TotalCount)> GetAllByCreatorAsync(Guid creatorId, DatasetQueryParameters @params);
    Task<Dataset?> GetByIdAsync(Guid id);
    Task<Dataset?> GetByNameAndCreatorAsync(string name, Guid creatorId);
    Task CreateAsync(Dataset dataset);
    Task UpdateAsync(Dataset dataset);
    Task DeleteAsync(Dataset dataset);
    Task SaveChangesAsync();
}

