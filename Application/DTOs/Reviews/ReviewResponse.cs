using DataLabelProject.Application.DTOs.Consensus;
using DataLabelProject.Business.Models.Enums;

namespace DataLabelProject.Application.DTOs.Reviews
{
    public class ReviewResponse
    {
        public Guid ReviewId { get; set; }
        public Guid ReviewerId { get; set; }
        public ReviewResult Result { get; set; }
        public string? Feedback { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public ConsensusResponse Consensus { get; set; } = null!;
    }

    public class TaskItemReviewsResponse
    {
        public Guid TaskItemId { get; set; }
        public Guid DatasetItemId { get; set; }
        public LabelingTaskItemStatus Status { get; set; }
        public int RevisionCount { get; set; }
        public List<ReviewResponse> Reviews { get; set; } = new();
    }
}