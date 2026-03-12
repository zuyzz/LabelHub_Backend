using DataLabelProject.Business.Models;
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

    public async Task<IEnumerable<Review>> GetByTaskIdAsync(Guid taskId)
    {
        return await _context.Reviews
            .AsNoTracking()
            .Where(r => r.TaskId == taskId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Review>> GetApprovedByTaskIdAsync(Guid taskId)
    {
        return await _context.Reviews
            .Where(r => r.TaskId == taskId && r.Result == "approved")
            .Include(r => r.ReviewAnnotation)
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