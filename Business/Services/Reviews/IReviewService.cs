using DataLabelProject.Application.DTOs.Reviews;

namespace DataLabelProject.Business.Services.Reviews
{
    public interface IReviewService
    {
        Task<ReviewResponse[]> BatchReviewAnnotationsAsync(BatchReviewRequest request);
        Task<IEnumerable<ReviewResponse>> GetReviewsForTaskAsync(Guid taskId);
        Task<(IEnumerable<ReviewResponse> Reviews, int TotalCount)> GetReviewsAsync(string? status, int page = 1, int pageSize = 10);
    }
}