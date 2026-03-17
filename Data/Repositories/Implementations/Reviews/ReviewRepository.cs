using System.Linq.Expressions;
using DataLabelProject.Application.DTOs.Annotations;
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

    public IQueryable<Review> Query()
    {
        return _context.Reviews;
    }

    public async Task<Review?> GetByIdAsync(Guid reviewId)
    {
        return await _context.Reviews
            .FirstOrDefaultAsync(r => r.ReviewId == reviewId);
    }

    public async Task CreateAsync(Review review)
    {
        await _context.Reviews.AddAsync(review);
    }

    public async Task CreateRangeAsync(IEnumerable<Review> reviews)
    {
        await _context.Reviews.AddRangeAsync(reviews);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}