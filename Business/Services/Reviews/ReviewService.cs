using DataLabelProject.Application.DTOs.Reviews;
using DataLabelProject.Business.Models;
using DataLabelProject.Business.Models.Enums;
using DataLabelProject.Business.Services.Users;
using DataLabelProject.Data;
using DataLabelProject.Data.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;
using ConsensusModel = DataLabelProject.Business.Models.Consensus;

namespace DataLabelProject.Business.Services.Reviews
{
    public class ReviewService : IReviewService
    {
        private readonly AppDbContext _context;
        private readonly IReviewRepository _reviewRepository;
        private readonly ICurrentUserService _currentUserService;

        public ReviewService(AppDbContext context, IReviewRepository reviewRepository, ICurrentUserService currentUserService)
        {
            _context = context;
            _reviewRepository = reviewRepository;
            _currentUserService = currentUserService;
        }

        public async Task<ReviewResponse> ReviewAnnotationAsync(CreateReviewRequest request)
        {
            var annotation = await _context.Annotations
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.AnnotationId == request.AnnotationId);

            if (annotation == null)
                throw new KeyNotFoundException("Annotation not found");

            var review = new Review
            {
                ReviewId = Guid.NewGuid(),
                AnnotationId = request.AnnotationId,
                ReviewerId = _currentUserService.UserId ?? throw new InvalidOperationException("User not authenticated"),
                Result = Enum.Parse<ReviewResult>(request.Result),
                Feedback = request.Feedback,
                ReviewedAt = DateTime.UtcNow,
                TaskId = annotation.TaskId
            };

            await _reviewRepository.CreateAsync(review);
            await _reviewRepository.SaveChangesAsync();

            // after inserting a new review we attempt to recompute the consensus for the task
            await EvaluateTaskConsensus(annotation.TaskId);

            return MapToResponse(review);
        }

        public async Task<IEnumerable<ReviewResponse>> GetReviewsForTaskAsync(Guid taskId)
        {
            var reviews = await _reviewRepository.GetByTaskIdAsync(taskId);
            return reviews.Select(MapToResponse);
        }

        private ReviewResponse MapToResponse(Review review)
        {
            return new ReviewResponse
            {
                ReviewId = review.ReviewId,
                AnnotationId = review.AnnotationId,
                ReviewerId = review.ReviewerId,
                Result = review.Result.ToString(),
                Feedback = review.Feedback,
                ReviewedAt = review.ReviewedAt,
                TaskId = review.TaskId
            };
        }

        private async Task EvaluateTaskConsensus(Guid taskId)
        {
            // Retrieve task and project configuration
            var task = await _context.LabelingTasks
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TaskId == taskId);

            if (task == null)
                return;

            var config = await _context.ProjectConfigs
                .AsNoTracking()
                .Where(c => c.ProjectId == task.ProjectId)
                .OrderByDescending(c => c.ProjectConfigId)
                .FirstOrDefaultAsync();

            if (config == null)
                return;

            // gather approved annotations (distinct by annotation id)
            var approvedReviews = (await _reviewRepository.GetApprovedByTaskIdAsync(taskId)).ToList();

            var uniqueAnnotations = approvedReviews
                .GroupBy(r => r.AnnotationId)
                .Select(g => g.First().ReviewAnnotation)
                .ToList();

            if (uniqueAnnotations.Count == config.AnnotationsPerSample)
                return;

            // compute majority agreement score based on payload string equality
            var payloadGroups = uniqueAnnotations
                .GroupBy(a => a.Payload)
                .Select(g => new { Payload = g.Key, Count = g.Count() });

            var best = payloadGroups.OrderByDescending(g => g.Count).First();
            double score = (double)best.Count / uniqueAnnotations.Count;

            if (score < config.AgreementThreshold)
                return;

            var consensusPayload = best.Payload;

            var existing = await _context.Consensuses
                .FirstOrDefaultAsync(c => c.TaskId == taskId);

            if (existing == null)
            {
                existing = new ConsensusModel
                {
                    ConsensusId = Guid.NewGuid(),
                    TaskId = taskId,
                    Payload = consensusPayload,
                    AgreementScore = score
                };

                await _context.Consensuses.AddAsync(existing);
            }
            else
            {
                existing.Payload = consensusPayload;
                existing.AgreementScore = score;
                _context.Consensuses.Update(existing);
            }

            await _context.SaveChangesAsync();
        }
    }
}