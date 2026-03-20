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

    public IQueryable<ProjectMember> Query()
    {
        return _context.ProjectMembers;
    }

    public async Task<List<ProjectMember>> GetByProjectIdAsync(Guid projectId)
    {
        return await _context.ProjectMembers
            .Where(pm => pm.ProjectId == projectId)
            .ToListAsync();
    }

    public async Task<ProjectMember?> GetByIdAsync(Guid projectId, Guid userId)
    {
        return await _context.ProjectMembers
            .Include(pm => pm.Project)
            .Include(pm => pm.ProjectMemberUser)
            .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.MemberId == userId);
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