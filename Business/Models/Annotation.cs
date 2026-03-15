using System;
using System.Collections.Generic;

namespace DataLabelProject.Business.Models;

public partial class Annotation
{
    public Guid AnnotationId { get; set; }

    public Guid TaskItemId { get; set; }

    public Guid AnnotatorId { get; set; }

    public string Payload { get; set; } = null!;

    public DateTime? SubmittedAt { get; set; }

    public string? Note { get; set; }

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual User AnnotationAnnotator { get; set; } = null!;

    public virtual LabelingTaskItem AnnotationTaskItem { get; set; } = null!;
}
