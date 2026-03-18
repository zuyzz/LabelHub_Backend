using DataLabelProject.Application.DTOs.Projects;
using DataLabelProject.Application.DTOs.Common;
using DataLabelProject.Application.DTOs.Users;

namespace DataLabelProject.Business.Services.Projects
{
    public interface IProjectMemberService
    {
        Task<PagedResponse<ProjectMemberResponse>> GetUserFromProject(Guid projectId, ProjectMemberQueryParameters @params);
        Task AddUserToProject(Guid userId, Guid projectId);
        Task RemoveUserFromProject(Guid userId, Guid projectId);
    }
}
