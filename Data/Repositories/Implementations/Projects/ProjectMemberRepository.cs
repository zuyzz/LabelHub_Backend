using DataLabelProject.Application.DTOs.Users;
using DataLabelProject.Business.Models;
using DataLabelProject.Data.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace DataLabelProject.Data.Repositories.Implementations.Projects;

public class ProjectMemberRepository : IProjectMemberRepository
{
    private readonly AppDbContext _context;

    public ProjectMemberRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ProjectMember?> GetByIdAsync(Guid projectId, Guid userId)
    {
        return await _context.ProjectMembers
            .Include(pm => pm.Project)
            .Include(pm => pm.ProjectMemberUser)
            .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.MemberId == userId);
    }

    public async Task<(IEnumerable<ProjectMember> Items, int TotalCount)> GetActiveMembersAsync(Guid projectId, UserQueryParameters @params)
    {
        var query = _context.ProjectMembers
            .Where(pm => pm.ProjectId == projectId && pm.ProjectMemberUser.IsActive)
            .Include(pm => pm.ProjectMemberUser)
            .ThenInclude(u => u.UserRole)
            .AsQueryable();

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip(@params.Offset)
            .Take(@params.PageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<ProjectMember> CreateAsync(ProjectMember projectMember)
    {
        await _context.ProjectMembers.AddAsync(projectMember);
        return projectMember;
    }

    public async Task DeleteAsync(ProjectMember projectMember)
    {
        _context.ProjectMembers.Remove(projectMember);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}