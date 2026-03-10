using DataLabelProject.Business.Models;
using DataLabelProject.Data.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace DataLabelProject.Data.Repositories.Implementations.ExportJobs;

public class ExportJobRepository : IExportJobRepository
{
    private readonly AppDbContext _context;

    public ExportJobRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ExportJob>> GetAllAsync()
    {
        return await _context.ExportJobs
            .AsNoTracking()
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();
    }

    public async Task<ExportJob?> GetByIdAsync(Guid exportId)
    {
        return await _context.ExportJobs
            .FirstOrDefaultAsync(e => e.ExportId == exportId);
    }

    public async Task<IEnumerable<ExportJob>> GetByProjectIdAsync(Guid projectId)
    {
        return await _context.ExportJobs
            .AsNoTracking()
            .Where(e => e.ProjectId == projectId)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();
    }

    public async Task CreateAsync(ExportJob exportJob)
    {
        await _context.ExportJobs.AddAsync(exportJob);
    }

    public async Task UpdateAsync(ExportJob exportJob)
    {
        _context.ExportJobs.Update(exportJob);
        await Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
