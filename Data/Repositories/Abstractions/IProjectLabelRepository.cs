using DataLabelProject.Business.Models;

namespace DataLabelProject.Data.Repositories.Abstractions;

public interface IProjectLabelRepository
{
    Task<ProjectLabel?> GetByIdAsync(Guid projectId, Guid labelId);
    Task<ProjectLabel> CreateAsync(ProjectLabel projectLabel);
    Task DeleteAsync(ProjectLabel projectLabel);
    Task SaveChangesAsync(); 
}
