using System.Linq.Expressions;
using DataLabelProject.Business.Models;

namespace DataLabelProject.Data.Repositories.Abstractions;

public interface IReviewRepository
{
    IQueryable<Review> Query();
    Task<Review?> GetByIdAsync(Guid reviewId);
    Task CreateAsync(Review review);
    Task CreateRangeAsync(IEnumerable<Review> reviews);
    Task SaveChangesAsync();
}