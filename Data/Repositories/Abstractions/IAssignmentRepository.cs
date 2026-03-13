using DataLabelProject.Business.Models;

namespace DataLabelProject.Data.Repositories.Abstractions;

public interface IAssignmentRepository
{
    Task<List<Assignment>> GetAllAsync();
    Task<Assignment?> GetByIdAsync(Guid assignmentId);
    Task<List<Assignment>> GetByAssignedToAsync(Guid userId);
    Task<Assignment?> GetByTaskIdAsync(Guid taskId);
    Task<Assignment?> GetByTaskIdAndUserAsync(Guid taskId, Guid userId);
    Task AddAsync(Assignment assignment);
    Task AddRangeAsync(IEnumerable<Assignment> assignments);
    Task UpdateAsync(Assignment assignment);
    Task UpdateRangeAsync(IEnumerable<Assignment> assignments);
    Task SaveChangesAsync();
}
