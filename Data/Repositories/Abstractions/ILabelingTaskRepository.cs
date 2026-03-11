using DataLabelProject.Business.Models;

namespace DataLabelProject.Data.Repositories.Abstractions;

public interface ILabelingTaskRepository
{
    Task<List<LabelingTask>> GetAllAsync();
    Task<LabelingTask?> GetByIdAsync(Guid taskId);
    Task<IEnumerable<LabelingTask>> GetAllByDatasetIdAsync(Guid datasetId);
    Task<List<LabelingTask>> GetByIdsAsync(IEnumerable<Guid> taskIds);
    Task<List<LabelingTask>> GetByProjectIdAsync(Guid projectId);
    Task AddAsync(LabelingTask task);
    Task AddRangeAsync(IEnumerable<LabelingTask> tasks);
    Task DeleteRangeAsync(IEnumerable<LabelingTask> task);
    Task SaveChangesAsync();
}
