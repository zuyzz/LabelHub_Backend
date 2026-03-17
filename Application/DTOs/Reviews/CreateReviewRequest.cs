using System.ComponentModel.DataAnnotations;
using DataLabelProject.Business.Models.Enums;

namespace DataLabelProject.Application.DTOs.Reviews
{
    public class CreateReviewRequest
    {
        [Required]
        public Guid TaskItemId { get; set; }
        
        [Required]
        public Guid ConsensusId { get; set; }

        [Required]
        public ReviewResult Result { get; set; }

        public string? Feedback { get; set; }
    }
}