using DataLabelProject.Business.Models;

namespace DataLabelProject.Data.Repositories.Abstractions;

public interface IGuidelineRepository
{
    Task<List<Guideline>> GetAllAsync();
    Task<Guideline?> GetByIdAsync(Guid id);
    
    /// <summary>
    /// Get guideline assigned to a specific project (if any)
    /// </summary>
    Task<Guideline?> GetByProjectIdAsync(Guid projectId);

    Task AddAsync(Guideline guideline);
    Task UpdateAsync(Guideline guideline);
    Task DeleteAsync(Guideline guideline);
}
