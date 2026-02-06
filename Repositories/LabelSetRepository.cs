using DataLabel_Project_BE.Data;
using DataLabel_Project_BE.Models;
using Microsoft.EntityFrameworkCore;

namespace DataLabel_Project_BE.Repositories;

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

    public async Task<LabelSet?> GetLatestVersionAsync(Guid labelSetId)
    {
        return await _context.LabelSets
            .Where(ls => ls.LabelSetId == labelSetId)
            .OrderByDescending(ls => ls.VersionNumber)
            .FirstOrDefaultAsync();
    }

    public async Task CreateAsync(LabelSet labelSet)
    {
        _context.LabelSets.Add(labelSet);
        await _context.SaveChangesAsync();
    }
}
