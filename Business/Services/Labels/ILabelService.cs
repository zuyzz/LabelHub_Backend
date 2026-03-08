using DataLabelProject.Business.Models;

namespace DataLabelProject.Business.Services.Labels;

public interface ILabelService
{
    Task<List<Label>> GetAllLabelsAsync();
    Task<List<Label>> GetLabelsByCategoryAsync(Guid categoryId);
    Task<Label> CreateLabelAsync(Guid categoryId, string name, Guid createdBy);
    Task UpdateLabelAsync(Guid labelId, string name, bool isActive);
    Task DeleteLabelAsync(Guid labelId);
}
