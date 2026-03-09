using DataLabelProject.Business.Models;
using DataLabelProject.Data.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace DataLabelProject.Data.Repositories.Implementations.Labels;

public class ProjectLabelRepository : IProjectLabelRepository
{
    private readonly AppDbContext _context;

    public ProjectLabelRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ProjectLabel?> GetByIdAsync(Guid projectId, Guid labelId)
    {
        return await _context.ProjectLabels
            .OrderByDescending(pl => pl.AttachedAt)
            .Include(pl => pl.Project)
            .Include(pl => pl.Label)
            .FirstOrDefaultAsync(pl => pl.ProjectId == projectId && pl.LabelId == labelId);
    }

    public async Task<ProjectLabel> CreateAsync(ProjectLabel projectLabel)
    {
        await _context.ProjectLabels.AddAsync(projectLabel);
        return projectLabel;
    }

    public async Task DeleteAsync(ProjectLabel projectLabel)
    {
        _context.ProjectLabels.Remove(projectLabel);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}

