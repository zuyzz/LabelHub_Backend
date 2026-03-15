using DataLabelProject.Business.Models;
using DataLabelProject.Business.Models.Enums;
using DataLabelProject.Data.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace DataLabelProject.Data.Repositories.Implementations.Reviews;

public class ReviewRepository : IReviewRepository
{
    private readonly AppDbContext _context;

    public ReviewRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Review?> GetByIdAsync(Guid reviewId)
    {
        return await _context.Reviews
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.ReviewId == reviewId);
    }

    public async Task<IEnumerable<Review>> GetByTaskItemIdAsync(Guid taskItemId)
    {
        return await _context.Reviews
            .AsNoTracking()
            .Where(r => r.TaskItemId == taskItemId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Review>> GetApprovedByTaskItemIdAsync(Guid taskItemId)
    {
        return await _context.Reviews
            .Where(r => r.TaskItemId == taskItemId && r.Result == ReviewResult.Approved)
            .Include(r => r.ReviewTaskItem)
                .ThenInclude(t => t.Annotation)
            .ToListAsync();
    }

    public async Task CreateAsync(Review review)
    {
        await _context.Reviews.AddAsync(review);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}