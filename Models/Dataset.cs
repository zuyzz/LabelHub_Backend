using System;
using System.Collections.Generic;

namespace DataLabel_Project_BE.Models;

public partial class Dataset
{
    public Guid DatasetId { get; set; }

    public Guid ProjectId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? StorageUri { get; set; }

    public string? Metadata { get; set; }

    public int VersionNumber { get; set; }

    public Guid? CurrentLabelSetId { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public virtual ICollection<AnnotationTask> AnnotationTasks { get; set; } = new List<AnnotationTask>();

    public virtual User? CreatedByUser { get; set; }

    public virtual LabelSet? CurrentLabelSet { get; set; }

    public virtual Project DatasetProject { get; set; } = null!;
}
