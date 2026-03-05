using DataLabelProject.Business.Models;

namespace DataLabelProject.Data.Repositories.Abstractions
{
    public interface IProjectTemplateRepository
    {
        Task<List<ProjectTemplate>> GetAllAsync();
        Task<ProjectTemplate?> GetByIdAsync(Guid id);
        Task<ProjectTemplate> CreateAsync(ProjectTemplate template);
        Task UpdateAsync(ProjectTemplate template);
        Task DeleteAsync(Guid id);
    }
}
