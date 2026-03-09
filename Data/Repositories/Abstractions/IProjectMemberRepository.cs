using DataLabelProject.Application.DTOs.Users;
using DataLabelProject.Business.Models;

namespace DataLabelProject.Data.Repositories.Abstractions;

public interface IProjectMemberRepository
{
    Task<(IEnumerable<ProjectMember> Items, int TotalCount)> GetActiveMembersAsync(Guid projectId, UserQueryParameters @params);
    Task<ProjectMember?> GetByIdAsync(Guid projectId, Guid userId);
    Task<ProjectMember> CreateAsync(ProjectMember projectMember);
    Task DeleteAsync(ProjectMember projectMember);
    Task SaveChangesAsync(); 
}
