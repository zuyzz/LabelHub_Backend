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
            .Include(a => a.AnnotationTaskItem)
            .ToListAsync();
    }

    public async Task<IEnumerable<Annotation>> GetByAnnotatorIdAsync(Guid annotatorId)
    {
        return await _context.Annotations
            .AsNoTracking()
            .Include(a => a.AnnotationTaskItem)
            .Where(a => a.AnnotatorId == annotatorId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Annotation>> GetByTaskItemIdAsync(Guid taskItemId)
    {
        return await _context.Annotations
            .Include(a => a.AnnotationTaskItem)
            .Where(a => a.TaskItemId == taskItemId)
            .ToListAsync();
    }

    public async Task<Annotation?> GetByIdAsync(Guid annotationId)
    {
        return await _context.Annotations
            .Include(a => a.AnnotationTaskItem)
            .FirstOrDefaultAsync(a => a.AnnotationId == annotationId);
    }

    public async Task<Annotation?> GetByTaskItemIdAndAnnotatorIdAsync(Guid taskItemId, Guid annotatorId)
    {
        return await _context.Annotations
            .Include(a => a.AnnotationTaskItem)
            .FirstOrDefaultAsync(a => a.TaskItemId == taskItemId && a.AnnotatorId == annotatorId);
    }

    public async Task AddAsync(Annotation annotation)
    {
        await _context.Annotations.AddAsync(annotation);
    }

    public Task UpdateAsync(Annotation annotation)
    {
        _context.Annotations.Update(annotation);
        return Task.CompletedTask;
    }

    public Task UpdateRangeAsync(IEnumerable<Annotation> annotations)
    {
        _context.Annotations.UpdateRange(annotations);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
