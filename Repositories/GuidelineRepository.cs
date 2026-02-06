using Microsoft.EntityFrameworkCore;
using DataLabel_Project_BE.Data;
using DataLabel_Project_BE.Models;
using DataLabel_Project_BE.Repositories;

namespace DataLabel_Project_BE.Repositories;

public class GuidelineRepository : IGuidelineRepository
{
    private readonly AppDbContext _context;

    public GuidelineRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Guideline>> GetAllAsync()
    {
        return await _context.Guidelines
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Guideline?> GetByIdAsync(Guid id)
    {
        return await _context.Guidelines.FindAsync(id);
    }

    public async Task AddAsync(Guideline guideline)
    {
        _context.Guidelines.Add(guideline);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Guideline guideline)
    {
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guideline guideline)
    {
        _context.Guidelines.Remove(guideline);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> IsGuidelineInUseAsync(Guid guidelineId)
    {
        return await _context.LabelSets
            .AnyAsync(ls => ls.GuidelineId == guidelineId);
    }
}
