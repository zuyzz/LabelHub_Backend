using DataLabelProject.Application.DTOs.Datasets;
using DataLabelProject.Business.Models;

namespace DataLabelProject.Data.Repositories.Abstractions;

public interface IDatasetRepository
{
    IQueryable<Dataset> Query();
    Task<Dataset?> GetByIdAsync(Guid id);
    Task<Dataset?> GetByNameAndCreatorAsync(string name, Guid creatorId);
    Task CreateAsync(Dataset dataset);
    Task UpdateAsync(Dataset dataset);
    Task DeleteAsync(Dataset dataset);
    Task SaveChangesAsync();
}

