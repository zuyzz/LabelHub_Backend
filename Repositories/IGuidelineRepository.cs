using DataLabel_Project_BE.Models;

namespace DataLabel_Project_BE.Repositories;

public interface IGuidelineRepository
{
    Task<List<Guideline>> GetAllAsync();
    Task<Guideline?> GetByIdAsync(Guid id);
    Task AddAsync(Guideline guideline);
    Task UpdateAsync(Guideline guideline);
    Task DeleteAsync(Guideline guideline);
    Task<bool> IsGuidelineInUseAsync(Guid guidelineId);
}
