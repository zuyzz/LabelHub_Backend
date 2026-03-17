using DataLabelProject.Application.DTOs.Annotations;
using DataLabelProject.Application.DTOs.Common;
using DataLabelProject.Application.DTOs.Reviews;

namespace DataLabelProject.Business.Services.Reviews
{
    public interface IReviewService
    {
        Task<PagedResponse<ReviewResponse>> GetReviewsAsync(ReviewQueryParameters @params);
        Task<PagedResponse<ReviewResponse>> GetReviewsByTaskAsync(Guid taskId, ReviewQueryParameters @params);
        Task<PagedResponse<ReviewResponse>> GetReviewsByTaskItemAsync(Guid taskItemId, ReviewQueryParameters @params);
        Task<ReviewResponse> CreateReviewAsync(CreateReviewRequest request);
    }
}