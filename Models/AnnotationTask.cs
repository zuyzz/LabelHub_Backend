using System;
using System.Collections.Generic;

namespace DataLabel_Project_BE.Models;

public partial class AnnotationTask
{
    public Guid TaskId { get; set; }

    public Guid DatasetId { get; set; }

    public string ScopeUri { get; set; } = null!;

    public string Status { get; set; } = null!;

    public string? Consensus { get; set; }

    public DateTime? DeadlineAt { get; set; }

    public DateTime? AssignedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Annotation> Annotations { get; set; } = new List<Annotation>();

    public virtual ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();

    public virtual Dataset TaskDataset { get; set; } = null!;
}
