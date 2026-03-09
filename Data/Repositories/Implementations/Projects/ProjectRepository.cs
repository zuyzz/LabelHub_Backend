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

    public async Task<(IEnumerable<Project> Items, int TotalCount)> GetAllAsync(ProjectQueryParameters @params)
    {
        var query = _context.Projects
            .AsNoTracking()
            .OrderByDescending(p => p.CreatedAt)
            .Include(p => p.ProjectCategory)
            .Include(p => p.ProjectTemplate)
            .AsQueryable();
        
        if (!string.IsNullOrWhiteSpace(@params.Name))
            query = query.Where(p => EF.Functions.ILike(p.Name, $"%{@params.Name.Trim()}%"));

        if (@params.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == @params.CategoryId.Value);

        if (@params.IsActive.HasValue)
            query = query.Where(p => p.IsActive == @params.IsActive.Value);

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip(@params.Offset)
            .Take(@params.PageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<(IEnumerable<Project> Items, int TotalCount)> GetAllByUserAsync(Guid userId, ProjectQueryParameters @params)
    {
        var query = _context.Projects
            .AsNoTracking()
            .OrderByDescending(p => p.CreatedAt)
            .Include(p => p.ProjectCategory)
            .Include(p => p.ProjectTemplate)
            .AsQueryable();

        query = query.Where(p => 
            _context.ProjectMembers.Any(pm => 
                pm.MemberId == userId && pm.ProjectId == p.ProjectId));
        
        if (!string.IsNullOrWhiteSpace(@params.Name))
            query = query.Where(p => EF.Functions.ILike(p.Name, $"%{@params.Name.Trim()}%"));

        if (@params.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == @params.CategoryId.Value);

        if (@params.IsActive.HasValue)
            query = query.Where(p => p.IsActive == @params.IsActive.Value);

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip(@params.Offset)
            .Take(@params.PageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<Project?> GetByIdAsync(Guid id)
    {
        return await _context.Projects
            .Include(p => p.ProjectCategory)
            .Include(p => p.ProjectTemplate)
            .FirstOrDefaultAsync(p => p.ProjectId == id);
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
