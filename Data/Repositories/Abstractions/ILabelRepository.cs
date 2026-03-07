using DataLabelProject.Business.Models;

namespace DataLabelProject.Data.Repositories.Abstractions;

public interface ILabelRepository
{
    Task<List<Label>> GetByLabelSetIdAsync(Guid labelSetId);
    Task<Label?> GetByIdAsync(Guid labelId);
    Task<Label?> GetByNameAndLabelSetAsync(Guid labelSetId, string name);
    Task AddAsync(Label label);
    Task UpdateAsync(Label label);
}
