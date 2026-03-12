using DataLabelProject.Business.Models;

namespace DataLabelProject.Data.Repositories.Abstractions;

public interface IReviewRepository
{
    Task<Review?> GetByIdAsync(Guid reviewId);
    Task<IEnumerable<Review>> GetByAnnotationIdAsync(Guid annotationId);
    Task CreateAsync(Review review);
    Task SaveChangesAsync();
}