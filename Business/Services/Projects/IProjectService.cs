using DataLabelProject.Application.DTOs.Projects;
using DataLabelProject.Application.DTOs.Common;

namespace DataLabelProject.Business.Services.Projects
{
    public interface IProjectService
    {
        Task<PagedResponse<ProjectResponse>> GetProjects(ProjectQueryParameters @params);
        Task<ProjectResponse?> GetProjectById(Guid id);
        Task<ProjectResponse> CreateProject(CreateProjectRequest request);
        Task<ProjectResponse?> UpdateProject(Guid id, UpdateProjectRequest request);
        Task<bool> DeleteProject(Guid id);
    }
}
