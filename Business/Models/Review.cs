using System;
using System.Collections.Generic;

namespace DataLabelProject.Business.Models;

public partial class Review
{
    public Guid ReviewId { get; set; }

    public Guid AnnotationId { get; set; }

    public Guid TaskId { get; set; }

    public Guid ReviewerId { get; set; }

    public string Result { get; set; } = null!;

    public string? Feedback { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public virtual Annotation ReviewAnnotation { get; set; } = null!;

    public virtual LabelingTask ReviewTask { get; set; } = null!;

    public virtual User ReviewUser { get; set; } = null!;
}
