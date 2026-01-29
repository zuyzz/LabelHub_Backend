using System;
using System.Collections.Generic;

namespace DataLabel_Project_BE.Models;

public partial class Review
{
    public Guid ReviewId { get; set; }

    public Guid AnnotationId { get; set; }

    public Guid ReviewerId { get; set; }

    public string? Result { get; set; }

    public string? Feedback { get; set; }

    public DateTime? DeadlineAt { get; set; }

    public bool? IsApproved { get; set; }

    public Guid? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public virtual Annotation ReviewAnnotation { get; set; } = null!;

    public virtual User? ApprovedByUser { get; set; }

    public virtual User ReviewUser { get; set; } = null!;
}
