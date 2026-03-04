using DataLabelProject.Business.Models;

namespace DataLabelProject.Data.Repositories.Abstractions;

public interface IDatasetRepository
{
    Task<Dataset> CreateDatasetAsync(Dataset dataset);
    Task AddDatasetItemsAsync(IEnumerable<DatasetItem> items);
    Task AddAnnotationTasksAsync(IEnumerable<AnnotationTask> tasks);
    Task<bool> ProjectExistsAsync(Guid projectId);
    Task SaveChangesAsync();
}
