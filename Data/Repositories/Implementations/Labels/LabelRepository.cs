using DataLabelProject.Data;
using DataLabelProject.Business.Models;
using DataLabelProject.Data.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;
using DataLabelProject.Application.DTOs.Labels;

namespace DataLabelProject.Data.Repositories.Implementations.Labels;

public class LabelRepository : ILabelRepository
{
    private readonly AppDbContext _context;

    public LabelRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<Label> Items, int TotalCount)> GetAllAsync(LabelQueryParameters @params)
    {
        var query = _context.Labels
            .AsNoTracking()
            .Include(l => l.LabelCategory)
            .Include(l => l.ProjectLabels)
            .AsQueryable();
        
        if (@params.CategoryId.HasValue) 
            query = query.Where(l => l.CategoryId == @params.CategoryId.Value);
        if (@params.ProjectId.HasValue)
            query = query.Where(l =>l.ProjectLabels
                .Any(pl => pl.ProjectId == @params.ProjectId.Value));

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip(@params.Offset)
            .Take(@params.PageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<Label?> GetByIdAsync(Guid labelId)
    {
        return await _context.Labels
            .Include(l => l.LabelCategory)
            .Include(l => l.ProjectLabels)
            .FirstOrDefaultAsync(l => l.LabelId == labelId);
    }

    public async Task CreateAsync(Label label)
    {
        await _context.Labels.AddAsync(label);
    }

    public async Task UpdateAsync(Label label)
    {
        _context.Labels.Update(label);
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

