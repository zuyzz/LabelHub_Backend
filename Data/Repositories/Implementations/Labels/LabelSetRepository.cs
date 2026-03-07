using DataLabelProject.Data;
using DataLabelProject.Business.Models;
using DataLabelProject.Data.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace DataLabelProject.Data.Repositories.Implementations.Labels;

public class LabelSetRepository : ILabelSetRepository
{
    private readonly AppDbContext _context;

    public LabelSetRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<LabelSet>> GetAllAsync()
    {
        return await _context.LabelSets
            .OrderByDescending(ls => ls.CreatedAt)
            .ToListAsync();
    }

    public async Task<LabelSet?> GetByIdAsync(Guid labelSetId)
    {
        return await _context.LabelSets.FindAsync(labelSetId);
    }

    public async Task CreateAsync(LabelSet labelSet)
    {
        _context.LabelSets.Add(labelSet);
        await _context.SaveChangesAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
