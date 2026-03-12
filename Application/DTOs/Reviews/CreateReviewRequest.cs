using System;
using System.ComponentModel.DataAnnotations;

namespace DataLabelProject.Application.DTOs.Reviews
{
    public class CreateReviewRequest
    {
        [Required]
        public Guid AnnotationId { get; set; }

        [Required]
        [RegularExpression("approved|rejected", ErrorMessage = "Result must be 'approved' or 'rejected'")]
        public string Result { get; set; } = null!;

        public string? Feedback { get; set; }
    }
}