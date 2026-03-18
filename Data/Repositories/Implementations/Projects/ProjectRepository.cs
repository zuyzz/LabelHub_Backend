using DataLabelProject.Business.Models;
using DataLabelProject.Data.Repositories.Abstractions;
using DataLabelProject.Application.DTOs.Projects;
using Microsoft.EntityFrameworkCore;

namespace DataLabelProject.Data.Repositories.Implementations.Projects;
public class ProjectRepository : IProjectRepository
{
    private readonly AppDbContext _context;

    public ProjectRepository(AppDbContext context)
    {
        _context = context;
    }

    public IQueryable<Project> Query()
    {
        return _context.Projects;
    }

    public async Task<Project?> GetByIdAsync(Guid id)
    {
        return await _context.Projects
            .Include(p => p.ProjectCategory)
            .FirstOrDefaultAsync(p => p.ProjectId == id);
    }

    public async Task<Project?> GetByNameAndCreatorAsync(string name, Guid userId)
    {
        return await _context.Projects
            .Include(p => p.ProjectCategory)
            .FirstOrDefaultAsync(p => p.Name.ToLower() == name.Trim().ToLower() && p.CreatedBy == userId);
    }

    public async Task CreateAsync(Project project)
    {
        await _context.Projects.AddAsync(project);
    }

    public async Task UpdateAsync(Project project)
    {
        _context.Projects.Update(project);
    }

    public async Task DeleteAsync(Project project)
    {
        _context.Projects.Remove(project);
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Projects.AnyAsync(p => p.ProjectId == id);
    }

    public async Task<IEnumerable<ProjectMember>> GetActiveProjectMembersAsync(Guid projectId)
    {
        return await _context.ProjectMembers
            .Where(pm => pm.ProjectId == projectId && pm.ProjectMemberUser.IsActive)
            .Include(pm => pm.ProjectMemberUser)
            .ThenInclude(u => u.UserRole)
            .ToListAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
