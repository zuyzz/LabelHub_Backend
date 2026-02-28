using System;
using System.Collections.Generic;

namespace DataLabelProject.Business.Models;

public partial class DatasetItem
{
    public Guid ItemId { get; set; }

    public Guid DatasetId { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? MediaType { get; set; }

    public string? StorageUri { get; set; }

    public virtual Dataset Dataset { get; set; } = null!;

    public virtual ICollection<AnnotationTask> AnnotationTasks { get; set; } = new List<AnnotationTask>();
}
