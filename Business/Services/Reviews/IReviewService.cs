using DataLabel_Project_BE.DTOs.Review;

namespace DataLabelProject.Business.Services.Reviews;

public interface IReviewService
{
    Task<(ReviewResponse? Review, string? ErrorMessage)> CreateReviewAsync(Guid reviewerId, CreateReviewRequest request);
}
