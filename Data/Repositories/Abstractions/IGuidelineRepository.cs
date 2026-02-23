using DataLabelProject.Business.Models;

namespace DataLabelProject.Data.Repositories.Abstractions;

public interface IGuidelineRepository
{
    Task<List<Guideline>> GetAllAsync();
    Task<Guideline?> GetByIdAsync(Guid id);
    Task AddAsync(Guideline guideline);
    Task UpdateAsync(Guideline guideline);
    Task DeleteAsync(Guideline guideline);
    Task<bool> IsGuidelineInUseAsync(Guid guidelineId);
}
