using DataLabelProject.Application.DTOs.Consensus;
using DataLabelProject.Application.DTOs.Tasks;
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
        public TaskItemResponse TaskItem { get; set; } = null!;
        public ConsensusResponse Consensus { get; set; } = null!;
    }
}