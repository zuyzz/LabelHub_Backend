using DataLabelProject.Business.Models;

namespace DataLabelProject.Data.Repositories.Abstractions;

public interface IExportJobRepository
{
    Task<IEnumerable<ExportJob>> GetAllAsync();
    Task<ExportJob?> GetByIdAsync(Guid exportId);
    Task<IEnumerable<ExportJob>> GetByProjectIdAsync(Guid projectId);
    Task CreateAsync(ExportJob exportJob);
    Task UpdateAsync(ExportJob exportJob);
    Task SaveChangesAsync();
}
