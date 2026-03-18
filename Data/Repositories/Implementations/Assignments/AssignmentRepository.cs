using DataLabelProject.Business.Models;
using DataLabelProject.Data.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace DataLabelProject.Data.Repositories.Implementations.Assignments;

public class AssignmentRepository : IAssignmentRepository
{
    private readonly AppDbContext _db;

    public AssignmentRepository(AppDbContext db)
    {
        _db = db;
    }

    public IQueryable<Assignment> Query()
    {
        return _db.Assignments;
    }

    public async Task<List<Assignment>> GetAllAsync()
    {
        return await _db.Assignments
            .Include(a => a.AssignmentTask)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<Assignment>> GetAvailableAsync()
    {
        var now = DateTime.UtcNow;
        return await _db.Assignments
            .Where(a => a.DeadlineAt > now && a.StartedAt < now)
            .ToListAsync();
    }

    public async Task<List<Assignment>> GetByAssignedToAsync(Guid userId)
    {
        return await _db.Assignments
            .Include(a => a.AssignmentTask)
            .Where(a => a.AssignedTo == userId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<Assignment>> GetAllByTaskIdAsync(Guid taskId)
    {
        return await _db.Assignments
            .Where(a => a.TaskId == taskId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Assignment?> GetByTaskIdAsync(Guid taskId)
    {
        return await _db.Assignments
            .FirstOrDefaultAsync(a => a.TaskId == taskId);
    }

    public async Task<Assignment?> GetByIdAsync(Guid id)
    {
        return await _db.Assignments
            .FirstOrDefaultAsync(a => a.AssignmentId == id);
    }

    public async Task<Assignment?> GetByTaskIdAndUserAsync(Guid taskId, Guid userId)
    {
        return await _db.Assignments
            .FirstOrDefaultAsync(a => a.TaskId == taskId && a.AssignedTo == userId);
    }

    public async Task AddAsync(Assignment assignment)
    {
        await _db.Assignments.AddAsync(assignment);
    }

    public async Task AddRangeAsync(List<Assignment> assignments)
    {
        await _db.Assignments.AddRangeAsync(assignments);
    }

    public async Task UpdateAsync(Assignment assignment)
    {
        _db.Assignments.Update(assignment);
    }

    public async Task UpdateRangeAsync(List<Assignment> assignments)
    {
        _db.Assignments.UpdateRange(assignments);
    }

    public async Task SaveChangesAsync()
    {
        await _db.SaveChangesAsync();
    }
}
