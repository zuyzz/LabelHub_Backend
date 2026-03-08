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

    public async Task<List<Assignment>> GetAllAsync()
    {
        return await _db.Assignments
            .Include(a => a.AssignmentTask)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<Assignment>> GetByAssignedToAsync(Guid userId)
    {
        return await _db.Assignments
            .Include(a => a.AssignmentTask)
                .ThenInclude(t => t.Assignments)
            .Where(a => a.AssignedTo == userId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Assignment?> GetByTaskIdAsync(Guid taskId)
    {
        return await _db.Assignments
            .FirstOrDefaultAsync(a => a.TaskId == taskId);
    }

    public async Task AddAsync(Assignment assignment)
    {
        await _db.Assignments.AddAsync(assignment);
    }

    public Task UpdateAsync(Assignment assignment)
    {
        _db.Assignments.Update(assignment);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _db.SaveChangesAsync();
    }
}
