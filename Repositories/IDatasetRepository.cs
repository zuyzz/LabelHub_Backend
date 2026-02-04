using DataLabel_Project_BE.Models;

namespace DataLabel_Project_BE.Repositories;

public interface IDatasetRepository
{
    Task<Dataset> CreateDatasetAsync(Dataset dataset);
    Task AddAnnotationTasksAsync(IEnumerable<AnnotationTask> tasks);
    Task<bool> ProjectExistsAsync(Guid projectId);
    Task SaveChangesAsync();
}