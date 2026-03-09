using DataLabelProject.Application.DTOs.Guidelines;
using DataLabelProject.Business.Models;

namespace DataLabelProject.Data.Repositories.Abstractions;

public interface IGuidelineRepository
{
    Task<(IEnumerable<Guideline> Items, int TotalCount)> GetAllAsync(GuidelineQueryParameters @params);
    Task<Guideline?> GetByIdAsync(Guid id);
    Task CreateAsync(Guideline guideline);
    Task UpdateAsync(Guideline guideline);
    Task DeleteAsync(Guideline guideline);
    Task SaveChangesAsync();
}
