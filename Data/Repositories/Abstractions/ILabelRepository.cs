using DataLabelProject.Application.DTOs.Labels;
using DataLabelProject.Business.Models;

namespace DataLabelProject.Data.Repositories.Abstractions;

public interface ILabelRepository
{
    IQueryable<Label> Query();
    Task<Label?> GetByIdAsync(Guid labelId);
    Task CreateAsync(Label label);
    Task UpdateAsync(Label label);
    Task SaveChangesAsync(); 
}
