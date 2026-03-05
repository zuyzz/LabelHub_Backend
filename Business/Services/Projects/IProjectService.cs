using DataLabelProject.Application.DTOs.Projects;
using DataLabelProject.Application.DTOs.Common;

namespace DataLabelProject.Business.Services.Projects
{
    public interface IProjectService
    {
        Task<PagedResponse<ProjectResponse>> GetProjectsAsync(ProjectQueryParameters query);
        Task<PagedResponse<ProjectResponse>> GetUserProjectsAsync(ProjectQueryParameters query);
        Task<IEnumerable<ProjectMemberResponse>> GetProjectMembersAsync(Guid projectId);
        Task<JoinProjectResult> JoinProjectAsync(Guid projectId);
        Task<IEnumerable<ProjectResponse>> GetAllAsync();
        Task<ProjectResponse?> GetByIdAsync(Guid id);
        Task<ProjectResponse> CreateAsync(ProjectCreateRequest dto);
        Task<ProjectResponse?> UpdateAsync(Guid id, ProjectUpdateRequest dto);
        Task<bool> DeleteAsync(Guid id);
    }
}
