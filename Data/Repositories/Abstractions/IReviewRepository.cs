using DataLabelProject.Business.Models;

namespace DataLabelProject.Data.Repositories.Abstractions;

public interface IReviewRepository
{
    Task<Review?> GetByIdAsync(Guid reviewId);
    Task<IEnumerable<Review>> GetByTaskIdAsync(Guid taskId);
    Task<IEnumerable<Review>> GetApprovedByTaskIdAsync(Guid taskId);
    Task CreateAsync(Review review);
    Task SaveChangesAsync();
}