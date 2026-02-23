using DataLabelProject.Business.Models;

namespace DataLabelProject.Data.Repositories.Abstractions;

public interface ILabelSetRepository
{
    Task<List<LabelSet>> GetAllAsync();
    Task<LabelSet?> GetLatestVersionAsync();
    Task CreateAsync(LabelSet labelSet);
    Task SaveChangesAsync();
}
