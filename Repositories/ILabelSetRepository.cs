using DataLabel_Project_BE.Models;

namespace DataLabel_Project_BE.Repositories;

public interface ILabelSetRepository
{
    Task<List<LabelSet>> GetAllAsync();
    Task<LabelSet?> GetLatestVersionAsync();
    Task CreateAsync(LabelSet labelSet);
    Task SaveChangesAsync();
}
