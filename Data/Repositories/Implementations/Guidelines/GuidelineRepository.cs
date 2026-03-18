using Microsoft.EntityFrameworkCore;
using DataLabelProject.Data;
using DataLabelProject.Business.Models;
using DataLabelProject.Data.Repositories.Abstractions;
using DataLabelProject.Application.DTOs.Guidelines;

namespace DataLabelProject.Data.Repositories.Implementations.Guidelines;

public class GuidelineRepository : IGuidelineRepository
{
    private readonly AppDbContext _context;

    public GuidelineRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<Guideline> Items, int TotalCount)> GetAllAsync(GuidelineQueryParameters @params)
    {
        var query = _context.Guidelines
            .AsNoTracking()
            .OrderByDescending(g => g.CreatedAt)
            .AsQueryable();

        if (!string.IsNullOrEmpty(@params.Content))
            query = query.Where(g => EF.Functions.ILike(g.Content, $"%{@params.Content}%"));
        
        var totalCount = await query.CountAsync();

        var items = await query
            .Skip(@params.Offset)
            .Take(@params.PageSize)
            .ToListAsync();
        
        return (items, totalCount);
    }

    public async Task<Guideline?> GetByIdAsync(Guid id)
    {
        return await _context.Guidelines
            .FirstOrDefaultAsync(g => g.GuidelineId == id);
    }

    public async Task<Guideline?> GetByProjectIdAsync(Guid projectId)
    {
        return await _context.Guidelines
            .FirstOrDefaultAsync(g => g.ProjectId == projectId);
    }

    public async Task CreateAsync(Guideline guideline)
    {
        await _context.Guidelines.AddAsync(guideline);
    }

    public async Task UpdateAsync(Guideline guideline)
    {
        _context.Update(guideline);
    }

    public async Task DeleteAsync(Guideline guideline)
    {
        _context.Guidelines.Remove(guideline);
    }

    public async Task SaveChangesAsync() {
        await _context.SaveChangesAsync();
    }
}
