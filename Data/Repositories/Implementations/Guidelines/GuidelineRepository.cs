using Microsoft.EntityFrameworkCore;
using DataLabelProject.Data;
using DataLabelProject.Business.Models;
using DataLabelProject.Data.Repositories.Abstractions;

namespace DataLabelProject.Data.Repositories.Implementations.Guidelines;

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
}
