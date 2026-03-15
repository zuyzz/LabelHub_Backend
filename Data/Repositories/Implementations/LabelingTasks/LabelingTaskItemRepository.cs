using DataLabelProject.Business.Models;
using DataLabelProject.Data.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace DataLabelProject.Data.Repositories.Implementations.LabelingTasks;

public class LabelingTaskItemRepository : ILabelingTaskItemRepository
{
    private readonly AppDbContext _context;

    public LabelingTaskItemRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<LabelingTaskItem>> GetAllAsync()
    {
        return await _context.LabelingTaskItems
            .AsNoTracking()
            .Include(ti => ti.DatasetItem)
            .Include(ti => ti.Project)
            .ToListAsync();
    }

    public async Task<List<LabelingTaskItem>> GetByIdsAsync(IEnumerable<Guid> ids)
    {
        return await _context.LabelingTaskItems
            .AsNoTracking()
            .Where(ti => ids.Contains(ti.TaskItemId))
            .Include(ti => ti.DatasetItem)
            .Include(ti => ti.Project)
            .ToListAsync();
    }

    public async Task<LabelingTaskItem?> GetByIdAsync(Guid id)
    {
        return await _context.LabelingTaskItems
            .Include(ti => ti.DatasetItem)
            .Include(ti => ti.Project)
            .FirstOrDefaultAsync(ti => ti.TaskItemId == id);
    }

    public async Task<List<LabelingTaskItem>> GetByDatasetItemIdsAsync(IEnumerable<Guid> datasetItemIds)
    {
        return await _context.LabelingTaskItems
            .AsNoTracking()
            .Where(ti => datasetItemIds.Contains(ti.DatasetItemId))
            .Include(ti => ti.DatasetItem)
            .Include(ti => ti.Project)
            .ToListAsync();
    }

    public async Task<List<LabelingTaskItem>> GetByProjectIdAsync(Guid projectId)
    {
        return await _context.LabelingTaskItems
            .AsNoTracking()
            .Where(ti => ti.ProjectId == projectId)
            .Include(ti => ti.DatasetItem)
            .Include(ti => ti.Project)
            .ToListAsync();
    }

    public async Task<List<LabelingTaskItem>> GetByTaskIdAsync(Guid taskId)
    {
        return await _context.LabelingTaskItems
            .Where(ti => ti.TaskId == taskId)
            .Include(ti => ti.DatasetItem)
            .Include(ti => ti.Annotations)
            .ToListAsync();
    }

    public async Task AddAsync(LabelingTaskItem taskItem)
    {
        await _context.LabelingTaskItems.AddAsync(taskItem);
    }

    public async Task AddRangeAsync(IEnumerable<LabelingTaskItem> taskItems)
    {
        await _context.LabelingTaskItems.AddRangeAsync(taskItems);
    }

    public async Task DeleteAsync(LabelingTaskItem taskItem)
    {
        _context.LabelingTaskItems.Remove(taskItem);
    }

    public async Task UpdateRangeAsync(IEnumerable<LabelingTaskItem> taskItems)
    {
        _context.LabelingTaskItems.UpdateRange(taskItems);
    }

    public async Task DeleteRangeAsync(IEnumerable<LabelingTaskItem> taskItems)
    {
        _context.LabelingTaskItems.RemoveRange(taskItems);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
