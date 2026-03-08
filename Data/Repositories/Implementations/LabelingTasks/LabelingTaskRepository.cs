using DataLabelProject.Business.Models;
using DataLabelProject.Data.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace DataLabelProject.Data.Repositories.Implementations.LabelingTasks;

public class LabelingTaskRepository : ILabelingTaskRepository
{
    private readonly AppDbContext _db;

    public LabelingTaskRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<LabelingTask>> GetAllAsync()
    {
        return await _db.LabelingTasks
            .Include(t => t.Assignments)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<LabelingTask?> GetByIdAsync(Guid taskId)
    {
        return await _db.LabelingTasks
            .Include(t => t.LabelingTaskProject)
            .FirstOrDefaultAsync(t => t.TaskId == taskId);
    }

    public async Task<List<LabelingTask>> GetByIdsAsync(IEnumerable<Guid> taskIds)
    {
        return await _db.LabelingTasks
            .Include(t => t.Assignments)
            .Where(t => taskIds.Contains(t.TaskId))
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<LabelingTask>> GetByProjectIdAsync(Guid projectId)
    {
        return await _db.LabelingTasks
            .Include(t => t.Assignments)
            .Where(t => t.ProjectId == projectId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task AddAsync(LabelingTask task)
    {
        await _db.LabelingTasks.AddAsync(task);
    }

    public async Task SaveChangesAsync()
    {
        await _db.SaveChangesAsync();
    }
}
