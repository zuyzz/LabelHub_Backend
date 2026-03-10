using DataLabelProject.Data;
using DataLabelProject.Business.Models;
using DataLabelProject.Data.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace DataLabelProject.Data.Repositories.Implementations.ProjectTemplates;

public class ProjectTemplateRepository : IProjectTemplateRepository
{
    private readonly AppDbContext _context;

    public ProjectTemplateRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ProjectTemplate>> GetAllAsync()
    {
        return await _context.ProjectTemplates.ToListAsync();
    }

    public async Task<ProjectTemplate?> GetByIdAsync(Guid id)
    {
        return await _context.ProjectTemplates
            .FirstOrDefaultAsync(t => t.TemplateId == id);
    }

    public async Task CreateAsync(ProjectTemplate template)
    {
        await _context.ProjectTemplates.AddAsync(template);
    }

    public async Task UpdateAsync(ProjectTemplate template)
    {
        _context.ProjectTemplates.Update(template);
    }

    public async Task DeleteAsync(ProjectTemplate template)
    {
        _context.ProjectTemplates.Remove(template);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
