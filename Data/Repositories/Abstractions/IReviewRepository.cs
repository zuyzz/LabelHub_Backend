using DataLabelProject.Business.Models;

namespace DataLabelProject.Data.Repositories.Abstractions;

public interface IReviewRepository
{
    Task<Review?> GetByIdAsync(Guid reviewId);
    Task<IEnumerable<Review>> GetByTaskItemIdAsync(Guid taskItemId);
    Task<IEnumerable<Review>> GetApprovedByTaskItemIdAsync(Guid taskItemId);
    Task CreateAsync(Review review);
    Task SaveChangesAsync();
}