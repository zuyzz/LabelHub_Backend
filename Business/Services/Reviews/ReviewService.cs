using DataLabelProject.Application.DTOs.Reviews;
using DataLabelProject.Business.Models;
using DataLabelProject.Business.Models.Enums;
using DataLabelProject.Business.Services.Users;
using DataLabelProject.Data;
using DataLabelProject.Data.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using ConsensusModel = DataLabelProject.Business.Models.Consensus;

namespace DataLabelProject.Business.Services.Reviews
{
    public class ReviewService : IReviewService
    {
        private readonly AppDbContext _context;
        private readonly IReviewRepository _reviewRepository;
        private readonly ILabelingTaskItemRepository _taskRepository;
        private readonly IAssignmentRepository _assignmentRepository;
        private readonly IProjectConfigRepository _projectConfigRepository;
        private readonly IConsensusRepository _consensusRepository;
        private readonly ICurrentUserService _currentUserService;

        public ReviewService(
            AppDbContext context,
            IReviewRepository reviewRepository,
            ILabelingTaskItemRepository taskRepository,
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

            var reviewerId = _currentUserService.UserId!.Value;

            var reviews = new List<Review>();
            foreach (var reviewItem in request.Reviews)
            {
                var annotation = task.Annotation;
                if (annotation == null || annotation.AnnotationId != reviewItem.AnnotationId)
                    throw new KeyNotFoundException($"Annotation {reviewItem.AnnotationId} not found for task");

                var review = new Review
                {
                    ReviewId = Guid.NewGuid(),
                    TaskItemId = request.TaskId,
                    ReviewerId = reviewerId,
                    Result = reviewItem.Result,
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
            bool allRejected = reviews.All(r => r.Result == ReviewResult.Rejected);

            if (allRejected)
            {
                // Increment revision count
                task.RevisionCount++;
                if (task.RevisionCount >= 3)
                {
                    task.Status = LabelingTaskItemStatus.Locked;
                }
                else
                {
                    // Reopen task: set assignments back to incompleted
                    var assignments = await _assignmentRepository.GetAllByTaskIdAsync(request.TaskId);
                    foreach (var assignment in assignments.Where(a => a.Status == AssignmentStatus.Completed))
                    {
                        assignment.Status = AssignmentStatus.Incompleted;
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
            var reviews = await _reviewRepository.GetByTaskItemIdAsync(taskId);
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
                AnnotationId = Guid.Empty, // Not stored in model
                ReviewerId = review.ReviewerId,
                Result = review.Result.ToString(),
                Feedback = review.Feedback,
                ReviewedAt = review.ReviewedAt,
                TaskId = review.TaskItemId
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
            var approvedReviews = (await _reviewRepository.GetApprovedByTaskItemIdAsync(taskId)).ToList();

            var uniqueAnnotations = approvedReviews
                .GroupBy(r => r.ReviewTaskItem.Annotation.AnnotationId)
                .Select(g => g.First().ReviewTaskItem.Annotation)
                .ToList();

            if (uniqueAnnotations.Count == 2)
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

            var existingConsensuses = await _consensusRepository.GetByDatasetItemIdAsync(taskId);
            var existing = existingConsensuses.FirstOrDefault();

            if (existing == null)
            {
                existing = new ConsensusModel
                {
                    ConsensusId = Guid.NewGuid(),
                    DatasetItemId = taskId,
                    Payload = JsonSerializer.Serialize(new { originalPayload = consensusPayload, agreementScore = score }),
                    CreatedAt = DateTime.UtcNow
                };

                await _consensusRepository.CreateAsync(existing);
            }
            else
            {
                existing.Payload = JsonSerializer.Serialize(new { originalPayload = consensusPayload, agreementScore = score });
                await _consensusRepository.UpdateAsync(existing);
            }
        }
    }
}