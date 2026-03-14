using DataLabelProject.Data;
using DataLabelProject.Business.Models;
using DataLabelProject.Business.Models.Enums;
using DataLabelProject.Data.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace DataLabelProject.Data.Repositories.Implementations.Annotations;

public class AnnotationRepository : IAnnotationRepository
{
    private readonly AppDbContext _context;

    public AnnotationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Annotation>> GetAllAsync()
    {
        return await _context.Annotations
            .AsNoTracking()
            .Include(a => a.Reviews)
            .ToListAsync();
    }

    public async Task<IEnumerable<Annotation>> GetByAnnotatorIdAsync(Guid annotatorId)
    {
        return await _context.Annotations
            .AsNoTracking()
            .Include(a => a.Reviews)
            .Where(a => a.AnnotatorId == annotatorId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Annotation>> GetApprovedByTaskIdAsync(Guid taskId)
    {
        return await _context.Annotations
            .AsNoTracking()
            .Include(a => a.Reviews)
            .Where(a => a.TaskId == taskId
                && a.Reviews
                    .OrderByDescending(r => r.ReviewedAt)
                    .First().Result == ReviewResult.Approved)
            .ToListAsync();
    }

    public async Task<Annotation?> GetByTaskIdAndAnnotatorIdAsync(Guid taskId, Guid annotatorId)
    {
        return await _context.Annotations
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.TaskId == taskId && a.AnnotatorId == annotatorId);
    }

    public async Task<Annotation?> GetByIdAsync(Guid annotationId)
    {
        return await _context.Annotations
            .Include(a => a.Reviews)
            .FirstOrDefaultAsync(a => a.AnnotationId == annotationId);
    }

    public async Task AddAsync(Annotation annotation)
    {
        await _context.Annotations.AddAsync(annotation);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
