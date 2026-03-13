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
        private readonly ILabelingTaskRepository _taskRepository;
        private readonly IAssignmentRepository _assignmentRepository;
        private readonly IProjectConfigRepository _projectConfigRepository;
        private readonly IConsensusRepository _consensusRepository;
        private readonly ICurrentUserService _currentUserService;

        public ReviewService(
            AppDbContext context,
            IReviewRepository reviewRepository,
            ILabelingTaskRepository taskRepository,
            IAssignmentRepository assignmentRepository,
            IProjectConfigRepository projectConfigRepository,
            IConsensusRepository consensusRepository,
            ICurrentUserService currentUserService)
        {
            _context = context;
            _reviewRepository = reviewRepository;
            _taskRepository = taskRepository;
            _assignmentRepository = assignmentRepository;
            _projectConfigRepository = projectConfigRepository;
            _consensusRepository = consensusRepository;
            _currentUserService = currentUserService;
        }

        public async Task<ReviewResponse[]> BatchReviewAnnotationsAsync(BatchReviewRequest request)
        {
            var task = await _taskRepository.GetByIdAsync(request.TaskId);
            if (task == null)
                throw new KeyNotFoundException("Task not found");

            var reviewerId = _currentUserService.UserId ?? throw new InvalidOperationException("User not authenticated");

            var reviews = new List<Review>();
            foreach (var reviewItem in request.Reviews)
            {
                var annotation = task.Annotations.FirstOrDefault(a => a.AnnotationId == reviewItem.AnnotationId);
                if (annotation == null)
                    throw new KeyNotFoundException($"Annotation {reviewItem.AnnotationId} not found for task");

                var review = new Review
                {
                    ReviewId = Guid.NewGuid(),
                    AnnotationId = reviewItem.AnnotationId,
                    TaskId = request.TaskId,
                    ReviewerId = reviewerId,
                    Result = Enum.Parse<ReviewResult>(reviewItem.Result),
                    Feedback = reviewItem.Feedback,
                    ReviewedAt = DateTime.UtcNow
                };
                reviews.Add(review);
            }

            // Save all reviews
            foreach (var review in reviews)
            {
                await _reviewRepository.CreateAsync(review);
            }
            await _reviewRepository.SaveChangesAsync();

            // Check if all annotations are rejected
            bool allRejected = reviews.All(r => r.Result == ReviewResult.rejected);

            if (allRejected)
            {
                // Increment revision count
                task.RevisionCount++;
                if (task.RevisionCount >= 3)
                {
                    task.Status = LabelingTaskStatus.removed;
                }
                else
                {
                    // Reopen task: set assignments back to incompleted
                    var assignments = await _assignmentRepository.GetAllByTaskIdAsync(request.TaskId);
                    foreach (var assignment in assignments.Where(a => a.Status == AssignmentStatus.completed))
                    {
                        assignment.Status = AssignmentStatus.incompleted;
                        await _assignmentRepository.UpdateAsync(assignment);
                    }
                    await _assignmentRepository.SaveChangesAsync();
                }
                await _taskRepository.SaveChangesAsync();
            }
            else
            {
                // Create consensus from approved annotations
                await CreateConsensusFromApproved(request.TaskId);
            }

            return reviews.Select(MapToResponse).ToArray();
        }

        public async Task<IEnumerable<ReviewResponse>> GetReviewsForTaskAsync(Guid taskId)
        {
            var reviews = await _reviewRepository.GetByTaskIdAsync(taskId);
            return reviews.Select(MapToResponse);
        }

        public async Task<(IEnumerable<ReviewResponse> Reviews, int TotalCount)> GetReviewsAsync(string? status, int page = 1, int pageSize = 10)
        {
            var query = _context.Reviews.AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                if (Enum.TryParse<ReviewResult>(status, out var result))
                {
                    query = query.Where(r => r.Result == result);
                }
            }

            var totalCount = await query.CountAsync();
            var reviews = await query
                .OrderByDescending(r => r.ReviewedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (reviews.Select(MapToResponse), totalCount);
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

        private async Task CreateConsensusFromApproved(Guid taskId)
        {
            // Retrieve task and project configuration
            var task = await _taskRepository.GetByIdAsync(taskId);
            if (task == null)
                return;

            var config = await _projectConfigRepository.GetLatestByProjectIdAsync(task.ProjectId);
            if (config == null)
                return;

            // gather approved annotations (distinct by annotation id)
            var approvedReviews = (await _reviewRepository.GetApprovedByTaskIdAsync(taskId)).ToList();

            var uniqueAnnotations = approvedReviews
                .GroupBy(r => r.AnnotationId)
                .Select(g => g.First().ReviewAnnotation)
                .ToList();

            if (uniqueAnnotations.Count < config.AnnotationsPerSample)
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

            var existingConsensuses = await _consensusRepository.GetByTaskIdAsync(taskId);
            var existing = existingConsensuses.FirstOrDefault();

            if (existing == null)
            {
                existing = new ConsensusModel
                {
                    ConsensusId = Guid.NewGuid(),
                    TaskId = taskId,
                    Payload = consensusPayload,
                    AgreementScore = score
                };

                await _consensusRepository.CreateAsync(existing);
            }
            else
            {
                existing.Payload = consensusPayload;
                existing.AgreementScore = score;
                await _consensusRepository.UpdateAsync(existing);
            }
        }
    }
}