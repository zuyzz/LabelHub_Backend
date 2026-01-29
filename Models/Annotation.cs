using System;
using System.Collections.Generic;

namespace DataLabel_Project_BE.Models;

public partial class Annotation
{
    public Guid AnnotationId { get; set; }

    public Guid TaskId { get; set; }

    public Guid AnnotatorId { get; set; }

    public Guid LabelSetId { get; set; }

    public int LabelSetVersionNumber { get; set; }

    public string AnnotationPayload { get; set; } = null!;

    public bool IsDraft { get; set; }

    public DateTime? SubmittedAt { get; set; }

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual User AnnotationAnnotator { get; set; } = null!;

    public virtual LabelSet AnnotationLabelSet { get; set; } = null!;

    public virtual AnnotationTask AnnotationTask { get; set; } = null!;
}
