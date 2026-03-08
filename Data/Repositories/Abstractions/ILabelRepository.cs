using DataLabelProject.Business.Models;

namespace DataLabelProject.Data.Repositories.Abstractions;

public interface ILabelRepository
{
    Task<List<Label>> GetAllAsync();
    Task<List<Label>> GetByCategoryIdAsync(Guid categoryId);
    Task<Label?> GetByIdAsync(Guid labelId);
    Task AddAsync(Label label);
    Task UpdateAsync(Label label);
}
