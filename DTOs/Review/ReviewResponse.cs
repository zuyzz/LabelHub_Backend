namespace DataLabel_Project_BE.DTOs.Review;

public class ReviewResponse
{
    public Guid ReviewId { get; set; }
    public Guid AnnotationId { get; set; }
    public Guid ReviewerId { get; set; }
    public bool IsApproved { get; set; }
    public string? Feedback { get; set; }
    public bool IsEscalated { get; set; }
    public DateTime ReviewedAt { get; set; }
}
