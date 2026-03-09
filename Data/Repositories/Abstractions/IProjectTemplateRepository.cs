using DataLabelProject.Business.Models;

namespace DataLabelProject.Data.Repositories.Abstractions
{
    public interface IProjectTemplateRepository
    {
        Task<IEnumerable<ProjectTemplate>> GetAllAsync();
        Task<ProjectTemplate?> GetByIdAsync(Guid id);
        Task CreateAsync(ProjectTemplate template);
        Task UpdateAsync(ProjectTemplate template);
        Task DeleteAsync(ProjectTemplate template);
        Task SaveChangesAsync();
    }
}
