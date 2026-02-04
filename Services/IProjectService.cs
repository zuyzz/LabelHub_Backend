using DataLabel_Project_BE.DTOs;

namespace DataLabel_Project_BE.Services
{
    public interface IProjectService
    {
        Task<PagedResponse<ProjectResponse>> GetProjectsAsync(ProjectQueryParameters query);        Task<DTOs.PagedResponse<DTOs.ProjectResponse>> GetUserProjectsAsync(DTOs.ProjectQueryParameters query);
        Task<DTOs.JoinProjectResult> JoinProjectAsync(Guid projectId);        Task<IEnumerable<ProjectResponse>> GetAllAsync();
        Task<ProjectResponse?> GetByIdAsync(Guid id);
        Task<ProjectResponse> CreateAsync(ProjectCreateRequest dto);
        Task<ProjectResponse?> UpdateAsync(Guid id, ProjectUpdateRequest dto);
        Task<bool> DeleteAsync(Guid id);
    }
}