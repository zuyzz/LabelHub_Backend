using DataLabelProject.Business.Models;
using DataLabelProject.Data.Repositories.Abstractions;
using DataLabel_Project_BE.DTOs.Review;
using System.Text.Json;

namespace DataLabelProject.Business.Services.Reviews;

public class ReviewService : IReviewService
{
    private readonly IAnnotationWorkflowRepository _repository;

    public ReviewService(IAnnotationWorkflowRepository repository)
    {
        _repository = repository;
    }

    public async Task<(ReviewResponse? Review, string? ErrorMessage)> CreateReviewAsync(Guid reviewerId, CreateReviewRequest request)
    {
        var reviewer = await _repository.GetUserByIdAsync(reviewerId);
        if (reviewer == null || !reviewer.IsActive)
        {
            return (null, "Reviewer not found or inactive");
        }

        var reviewerRole = await _repository.GetRoleByIdAsync(reviewer.RoleId);
        if (reviewerRole == null || reviewerRole.RoleName.ToLower() != "reviewer")
        {
            return (null, "Only Reviewer role can review annotations");
        }

        var annotation = await _repository.GetAnnotationWithContextAsync(request.AnnotationId);
        if (annotation == null)
        {
            return (null, "Annotation not found");
        }

        if (annotation.AnnotatorId == reviewerId)
        {
            return (null, "Self-review is not allowed");
        }

        if (!request.IsApproved && string.IsNullOrWhiteSpace(request.Feedback))
        {
            return (null, "Feedback is required when rejecting");
        }

        int rejectLimit = (await _repository.GetSystemConfigAsync())?.RejectLimit ?? 3;
        int currentRejectCount = await _repository.CountRejectedReviewsAsync(request.AnnotationId);
        bool isEscalated = !request.IsApproved && (currentRejectCount + 1) >= rejectLimit;

        var review = new Review
        {
            ReviewId = Guid.NewGuid(),
            AnnotationId = request.AnnotationId,
            ReviewerId = reviewerId,
            IsApproved = request.IsApproved,
            Result = request.IsApproved ? "approved" : "rejected",
            Feedback = request.Feedback,
            ReviewedAt = DateTime.UtcNow,
            ApprovedBy = request.IsApproved ? reviewerId : null,
            ApprovedAt = request.IsApproved ? DateTime.UtcNow : null
        };

        _repository.AddReview(review);

        if (!request.IsApproved)
        {
            annotation.AnnotationTask.Status = "rejected";
        }

        var projectId = await _repository.GetProjectIdByAnnotationIdAsync(request.AnnotationId);
        _repository.AddActivityLog(new ActivityLog
        {
            ActivityLogId = Guid.NewGuid(),
            ProjectId = projectId,
            UserId = reviewerId,
            EventType = "ANNOTATION_REVIEWED",
            TargetEntity = "Review",
            TargetId = review.ReviewId,
            Details = JsonSerializer.Serialize(new
            {
                annotationId = request.AnnotationId,
                isApproved = request.IsApproved,
                reviewId = review.ReviewId,
                isEscalated
            }),
            CreatedAt = DateTime.UtcNow
        });

        await _repository.SaveChangesAsync();

        return (new ReviewResponse
        {
            ReviewId = review.ReviewId,
            AnnotationId = review.AnnotationId,
            ReviewerId = review.ReviewerId,
            IsApproved = review.IsApproved ?? false,
            Feedback = review.Feedback,
            IsEscalated = isEscalated,
            ReviewedAt = review.ReviewedAt ?? DateTime.UtcNow
        }, null);
    }
}
