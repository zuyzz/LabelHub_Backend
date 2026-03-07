using DataLabelProject.Business.Models;

namespace DataLabelProject.Business.Services.Labels;

public interface ILabelService
{
    Task<List<Label>> GetLabelsByLabelSetAsync(Guid labelSetId);
    Task<Label> CreateLabelAsync(Guid labelSetId, string name);
    Task UpdateLabelAsync(Guid labelId, string name, bool isActive);
    Task DeleteLabelAsync(Guid labelId);
    Task<LabelSet?> GetLabelSetByIdAsync(Guid labelSetId);
}
