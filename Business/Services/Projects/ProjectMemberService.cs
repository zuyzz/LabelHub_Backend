using DataLabelProject.Application.DTOs.Projects;
using DataLabelProject.Application.DTOs.Common;
using DataLabelProject.Business.Models;
using DataLabelProject.Data.Repositories.Abstractions;
using DataLabelProject.Business.Services.Users;
using DataLabelProject.Application.DTOs.Users;

namespace DataLabelProject.Business.Services.Projects;

public class ProjectMemberService : IProjectMemberService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectMemberRepository _projectMemberRepository;

    public ProjectMemberService(
        IProjectRepository projectRepository,
        IProjectMemberRepository projectMemberRepository)
    {
        _projectRepository = projectRepository;
        _projectMemberRepository = projectMemberRepository;
    }

    public async Task AddUserToProject(Guid userId, Guid projectId)
    {
        var existing = await _projectMemberRepository.GetByIdAsync(projectId, userId);
        if (existing != null)
            throw new InvalidOperationException("User already in project");

        var member = new ProjectMember
        {
            ProjectId = projectId,
            MemberId = userId,
            JoinedAt = DateTime.UtcNow
        };

        await _projectMemberRepository.CreateAsync(member);
        await _projectMemberRepository.SaveChangesAsync();
    }

    public async Task RemoveUserFromProject(Guid userId, Guid projectId)
    {
        var member = await _projectMemberRepository.GetByIdAsync(projectId, userId);
        if (member == null)
            throw new KeyNotFoundException("Project member not found");

        await _projectMemberRepository.DeleteAsync(member);
        await _projectMemberRepository.SaveChangesAsync();
    }

    public async Task<PagedResponse<ProjectMemberResponse>?> GetUserFromProject(Guid projectId, UserQueryParameters @params)
    {
        var project = await _projectRepository.GetByIdAsync(projectId);
        if (project == null) return null;

        var (items, totalCount) = await _projectMemberRepository.GetActiveMembersAsync(projectId, @params);

        return new PagedResponse<ProjectMemberResponse>
        {
            Items = items.Select(MapToResponse).ToList(),
            TotalItems = totalCount,
            Page = @params.Page,
            PageSize = @params.PageSize,
        };
    }

    private static ProjectMemberResponse MapToResponse(ProjectMember p) =>
        new ProjectMemberResponse
        {
            UserId = p.ProjectMemberUser.UserId,
            Username = p.ProjectMemberUser.Username,
            DisplayName = p.ProjectMemberUser.DisplayName,
            Email = p.ProjectMemberUser.Email,
            PhoneNumber = p.ProjectMemberUser.PhoneNumber,
            RoleId = p.ProjectMemberUser.UserRole.RoleId,
            RoleName = p.ProjectMemberUser.UserRole.RoleName,
            IsActive = p.ProjectMemberUser.IsActive,
            JoinedAt = p.JoinedAt
        };
}