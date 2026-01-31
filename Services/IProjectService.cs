using DataLabel_Project_BE.DTOs;

namespace DataLabel_Project_BE.Services
{
    public interface IProjectService
    {
        Task<IEnumerable<ProjectResponse>> GetAllAsync();
        Task<ProjectResponse?> GetByIdAsync(Guid id);
        Task<ProjectResponse> CreateAsync(ProjectCreateRequest dto);
        Task<ProjectResponse?> UpdateAsync(Guid id, ProjectUpdateRequest dto);
        Task<bool> DeleteAsync(Guid id);
    }
}