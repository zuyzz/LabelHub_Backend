using DataLabelProject.Application.DTOs.Users;
using DataLabelProject.Business.Models;

namespace DataLabelProject.Data.Repositories.Abstractions;

public interface IProjectMemberRepository
{
    IQueryable<ProjectMember> Query();
    Task<ProjectMember?> GetByIdAsync(Guid projectId, Guid userId);
    Task<ProjectMember> CreateAsync(ProjectMember projectMember);
    Task DeleteAsync(ProjectMember projectMember);
    Task SaveChangesAsync(); 
}
