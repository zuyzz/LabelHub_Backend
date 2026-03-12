using System;

namespace DataLabelProject.Application.DTOs.Reviews
{
    public class ReviewResponse
    {
        public Guid ReviewId { get; set; }
        public Guid AnnotationId { get; set; }
        public Guid ReviewerId { get; set; }
        public string Result { get; set; } = null!;
        public string? Feedback { get; set; }
        public DateTime? ReviewedAt { get; set; }
        }
}