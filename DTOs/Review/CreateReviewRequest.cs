using System.ComponentModel.DataAnnotations;

namespace DataLabel_Project_BE.DTOs.Review;

public class CreateReviewRequest
{
    [Required]
    public Guid AnnotationId { get; set; }

    [Required]
    public bool IsApproved { get; set; }

    public string? Feedback { get; set; }
}
