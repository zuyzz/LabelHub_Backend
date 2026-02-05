using DataLabel_Project_BE.Models;

namespace DataLabel_Project_BE.Services;

public interface ILabelService
{
    Task<List<Label>> GetLabelsByLabelSetAsync(Guid labelSetId);
    Task<Label> CreateLabelAsync(Guid labelSetId, string name);
    Task UpdateLabelAsync(Guid labelId, string name, bool isActive);
    Task DeleteLabelAsync(Guid labelId);
}
