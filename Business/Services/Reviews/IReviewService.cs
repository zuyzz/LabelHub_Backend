using DataLabelProject.Application.DTOs.Reviews;

namespace DataLabelProject.Business.Services.Reviews
{
    public interface IReviewService
    {
        Task<ReviewResponse> ReviewAnnotationAsync(CreateReviewRequest request);
        Task<IEnumerable<ReviewResponse>> GetReviewsForTaskAsync(Guid taskId);
    }
}