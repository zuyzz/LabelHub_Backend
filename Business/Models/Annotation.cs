using System;
using System.Collections.Generic;

namespace DataLabelProject.Business.Models;

public partial class Annotation
{
    public Guid AnnotationId { get; set; }

    // NB: database stores datasetItemId/projectId rather than taskId
    public Guid DatasetItemId { get; set; }
    public Guid ProjectId { get; set; }

    public Guid AnnotatorId { get; set; }

    public string Payload { get; set; } = null!;

    public DateTime? SubmittedAt { get; set; }

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual User AnnotationAnnotator { get; set; } = null!;

    public virtual DatasetItem AnnotationDatasetItem { get; set; } = null!;

    public virtual Project AnnotationProject { get; set; } = null!;
}
