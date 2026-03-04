using DataLabelProject.Business.Models;
using DataLabelProject.Data.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace DataLabelProject.Data.Repositories.Implementations.Annotations;

public class AnnotationWorkflowRepository : IAnnotationWorkflowRepository
{
    private readonly AppDbContext _context;

    public AnnotationWorkflowRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AnnotationTask?> GetTaskWithDatasetContextAsync(Guid taskId)
    {
        return await _context.AnnotationTasks
            .Include(t => t.TaskDatasetItem)
                .ThenInclude(i => i.Dataset)
                    .ThenInclude(d => d.CurrentLabelSet)
            .Include(t => t.Assignments)
            .FirstOrDefaultAsync(t => t.TaskId == taskId && !t.Deleted);
    }

    public async Task<Annotation?> GetAnnotationByTaskAndAnnotatorAsync(Guid taskId, Guid annotatorId)
    {
        return await _context.Annotations
            .FirstOrDefaultAsync(a => a.TaskId == taskId && a.AnnotatorId == annotatorId);
    }

    public async Task<Annotation?> GetAnnotationWithContextAsync(Guid annotationId)
    {
        return await _context.Annotations
            .Include(a => a.AnnotationTask)
                .ThenInclude(t => t.TaskDatasetItem)
            .FirstOrDefaultAsync(a => a.AnnotationId == annotationId);
    }

    public async Task<User?> GetUserByIdAsync(Guid userId)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
    }

    public async Task<Role?> GetRoleByIdAsync(Guid roleId)
    {
        return await _context.Roles.FirstOrDefaultAsync(r => r.RoleId == roleId);
    }

    public async Task<SystemConfig?> GetSystemConfigAsync()
    {
        return await _context.SystemConfigs.AsNoTracking().FirstOrDefaultAsync();
    }

    public async Task<int> CountRejectedReviewsAsync(Guid annotationId)
    {
        return await _context.Reviews.CountAsync(r => r.AnnotationId == annotationId && r.IsApproved == false);
    }

    public async Task<Guid> GetProjectIdByTaskIdAsync(Guid taskId)
    {
        return await _context.AnnotationTasks
            .Where(t => t.TaskId == taskId)
            .Select(t => t.TaskDatasetItem.Dataset.ProjectId)
            .FirstOrDefaultAsync();
    }

    public async Task<Guid> GetProjectIdByAnnotationIdAsync(Guid annotationId)
    {
        return await _context.Annotations
            .Where(a => a.AnnotationId == annotationId)
            .Select(a => a.AnnotationTask.TaskDatasetItem.Dataset.ProjectId)
            .FirstOrDefaultAsync();
    }

    public void AddAnnotation(Annotation annotation)
    {
        _context.Annotations.Add(annotation);
    }

    public void AddReview(Review review)
    {
        _context.Reviews.Add(review);
    }

    public void AddActivityLog(ActivityLog log)
    {
        _context.ActivityLogs.Add(log);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
