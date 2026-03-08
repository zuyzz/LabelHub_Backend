using DataLabelProject.Business.Models;
using DataLabelProject.Data.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace DataLabelProject.Data.Repositories.Implementations.AnnotationTasks;

public class AnnotationTaskRepository : IAnnotationTaskRepository
{
    private readonly AppDbContext _context;

    public AnnotationTaskRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DatasetItem?> GetDatasetItemByIdAsync(Guid datasetItemId)
    {
        return await _context.DatasetItems
            .AsNoTracking()
            .FirstOrDefaultAsync(di => di.ItemId == datasetItemId);
    }

    public async Task<Dataset?> GetDatasetByIdAsync(Guid datasetId)
    {
        return await _context.Datasets
            .AsNoTracking()
            .Where(d => d.DatasetId == datasetId)
            .Select(d => new Dataset
            {
                DatasetId = d.DatasetId,
                Name = d.Name
            })
            .FirstOrDefaultAsync();
    }

    public async Task<Project?> GetProjectByIdAsync(Guid projectId)
    {
        return await _context.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.ProjectId == projectId);
    }

    public async Task<SystemConfig?> GetSystemConfigAsync()
    {
        return await _context.SystemConfigs
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    public async Task<AnnotationTask?> GetTaskWithDatasetItemAsync(Guid taskId)
    {
        return await _context.AnnotationTasks
            .Include(t => t.TaskDatasetItem)
            .FirstOrDefaultAsync(t => t.TaskId == taskId && !t.Deleted);
    }

    public async Task<AnnotationTask?> GetTaskWithAssignmentsAsync(Guid taskId)
    {
        return await _context.AnnotationTasks
            .Include(t => t.TaskDatasetItem)
            .Include(t => t.Assignments)
            .FirstOrDefaultAsync(t => t.TaskId == taskId && !t.Deleted);
    }

    public async Task<AnnotationTask?> GetTaskByIdWithRelationsAsync(Guid taskId)
    {
        return await _context.AnnotationTasks
            .Include(t => t.TaskDatasetItem)
            .Include(t => t.Assignments)
                .ThenInclude(a => a.AssignmentUser)
            .Include(t => t.Assignments)
                .ThenInclude(a => a.AssignedByUser)
            .FirstOrDefaultAsync(t => t.TaskId == taskId && !t.Deleted);
    }

    public async Task<List<AnnotationTask>> GetTasksForManagerAsync()
    {
        return await _context.AnnotationTasks
            .Where(t => !t.Deleted)
            .Include(t => t.TaskDatasetItem)
            .Include(t => t.Assignments)
                .ThenInclude(a => a.AssignmentUser)
            .Include(t => t.Assignments)
                .ThenInclude(a => a.AssignedByUser)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<AnnotationTask>> GetTasksForAnnotatorAsync(Guid annotatorId)
    {
        return await _context.AnnotationTasks
            .Where(t => !t.Deleted)
            .Include(t => t.TaskDatasetItem)
            .Include(t => t.Assignments)
                .ThenInclude(a => a.AssignmentUser)
            .Include(t => t.Assignments)
                .ThenInclude(a => a.AssignedByUser)
            .Where(t => t.Assignments.Any(a => a.AssignedTo == annotatorId))
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> HasAssignmentAsync(Guid taskId)
    {
        return await _context.Assignments
            .AnyAsync(a => a.TaskId == taskId);
    }

    public async Task<User?> GetUserByIdAsync(Guid userId)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
    }

    public async Task<Role?> GetRoleByIdAsync(Guid roleId)
    {
        return await _context.Roles.FirstOrDefaultAsync(r => r.RoleId == roleId);
    }

    public async Task<Guid> GetProjectIdByDatasetItemIdAsync(Guid datasetItemId)
    {
        await Task.CompletedTask;
        return Guid.Empty;
    }

    public async Task<List<Assignment>> GetAssignmentsByTaskIdAsync(Guid taskId)
    {
        return await _context.Assignments
            .Where(a => a.TaskId == taskId)
            .ToListAsync();
    }

    public void AddTask(AnnotationTask task)
    {
        _context.AnnotationTasks.Add(task);
    }

    public void AddAssignment(Assignment assignment)
    {
        _context.Assignments.Add(assignment);
    }

    public void AddActivityLog(ActivityLog activityLog)
    {
        _context.ActivityLogs.Add(activityLog);
    }

    public void RemoveAssignments(IEnumerable<Assignment> assignments)
    {
        _context.Assignments.RemoveRange(assignments);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
