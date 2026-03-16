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
            .Include(t => t.LabelingTaskProject)
            .Include(t => t.TaskItems)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<LabelingTask?> GetByIdAsync(Guid taskId)
    {
        return await _db.LabelingTasks
            .Include(t => t.Assignments)
            .Include(t => t.LabelingTaskProject)
            .Include(t => t.TaskItems)
            .FirstOrDefaultAsync(t => t.TaskId == taskId);
    }

    public async Task<List<LabelingTask>> GetByIdsAsync(IEnumerable<Guid> taskIds)
    {
        return await _db.LabelingTasks
            .Include(t => t.Assignments)
            .Include(t => t.TaskItems)
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

    public async Task<List<LabelingTask>> GetByDatasetItemIdsAsync(IEnumerable<Guid> datasetItemIds)
    {
        var taskIds = await _db.LabelingTaskItems
            .Where(ti => datasetItemIds.Contains(ti.DatasetItemId) && ti.TaskId.HasValue)
            .Select(ti => ti.TaskId!.Value)
            .Distinct()
            .ToListAsync();

        if (taskIds.Count == 0)
            return new List<LabelingTask>();

        return await GetByIdsAsync(taskIds);
    }

    public async Task AddAsync(LabelingTask task)
    {
        await _db.LabelingTasks.AddAsync(task);
    }

    public async Task AddRangeAsync(IEnumerable<LabelingTask> tasks)
    {
        await _db.LabelingTasks.AddRangeAsync(tasks);
    }

    public async Task DeleteRangeAsync(IEnumerable<LabelingTask> tasks)
    {
        _db.LabelingTasks.RemoveRange(tasks);
    }

    public async Task SaveChangesAsync()
    {
        await _db.SaveChangesAsync();
    }
}
