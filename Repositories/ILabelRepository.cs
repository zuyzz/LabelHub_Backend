using DataLabel_Project_BE.Models;

namespace DataLabel_Project_BE.Repositories;

public interface ILabelRepository
{
    Task<List<Label>> GetByLabelSetIdAsync(Guid labelSetId);
    Task<Label?> GetByIdAsync(Guid labelId);
    Task AddAsync(Label label);
    Task UpdateAsync(Label label);
}
