using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataLabelProject.Application.DTOs.Reviews
{
    public class BatchReviewRequest
    {
        [Required]
        public Guid TaskId { get; set; }

        [Required]
        public List<ReviewItem> Reviews { get; set; } = new List<ReviewItem>();
    }

    public class ReviewItem
    {
        [Required]
        public Guid AnnotationId { get; set; }

        [Required]
        [RegularExpression("approved|rejected", ErrorMessage = "Result must be 'approved' or 'rejected'")]
        public string Result { get; set; } = null!;

        public string? Feedback { get; set; }
    }
}