using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DataLabelProject.Business.Models.Enums;

namespace DataLabelProject.Application.DTOs.Reviews
{
    public class BatchReviewRequest
    {
        [Required]
        public Guid TaskItemId { get; set; }

        [Required]
        public List<ReviewItem> Reviews { get; set; } = new List<ReviewItem>();
    }

    public class ReviewItem
    {
        [Required]
        public Guid ConsensusId { get; set; }

        [Required]
        [RegularExpression("approved|rejected", ErrorMessage = "Result must be 'approved' or 'rejected'")]
        public ReviewResult Result { get; set; }

        public string? Feedback { get; set; }
    }
}