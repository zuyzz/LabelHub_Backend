using DataLabelProject.Business.Models;

namespace DataLabelProject.Data.Repositories.Abstractions;

public interface IAssignmentRepository
{
    Task<List<Assignment>> GetAllAsync();
    Task<List<Assignment>> GetByAssignedToAsync(Guid userId);
    Task<Assignment?> GetByTaskIdAsync(Guid taskId);
    Task AddAsync(Assignment assignment);
    Task UpdateAsync(Assignment assignment);
    Task SaveChangesAsync();
}
