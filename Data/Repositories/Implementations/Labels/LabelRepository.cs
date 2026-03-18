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

    public IQueryable<Label> Query()
    {
        return _context.Labels;
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

