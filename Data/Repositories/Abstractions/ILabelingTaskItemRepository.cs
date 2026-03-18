using DataLabelProject.Business.Models;

namespace DataLabelProject.Data.Repositories.Abstractions;

public interface ILabelingTaskItemRepository
{
    IQueryable<LabelingTaskItem> Query();
    Task<IEnumerable<LabelingTaskItem>> GetAllAsync();
    Task<LabelingTaskItem?> GetByIdAsync(Guid id);
    Task<List<LabelingTaskItem>> GetByIdsAsync(IEnumerable<Guid> ids);
    Task<List<LabelingTaskItem>> GetByDatasetItemIdsAsync(IEnumerable<Guid> datasetItemIds);
    Task<List<LabelingTaskItem>> GetUnassignedByDatasetItemIdsAsync(IEnumerable<Guid> datasetItemIds);
    Task<List<LabelingTaskItem>> GetByProjectIdAsync(Guid projectId);
    Task<List<LabelingTaskItem>> GetByTaskIdAsync(Guid taskId);
    Task AddAsync(LabelingTaskItem taskItem);
    Task DeleteAsync(LabelingTaskItem taskItem);
    Task AddRangeAsync(IEnumerable<LabelingTaskItem> taskItems);
    Task UpdateRangeAsync(IEnumerable<LabelingTaskItem> taskItems);
    Task DeleteRangeAsync(IEnumerable<LabelingTaskItem> taskItems);
    Task SaveChangesAsync();
}
