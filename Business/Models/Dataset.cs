using System;
using System.Collections.Generic;
using DataLabelProject.Business.Models.Enums;

namespace DataLabelProject.Business.Models;

public partial class Dataset
{
    public Guid DatasetId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? Metadata { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public MediaType MediaType { get; set; } = MediaType.image;

    public virtual ICollection<AnnotationTask> AnnotationTasks { get; set; } = new List<AnnotationTask>();

    public virtual ICollection<DatasetItem> DatasetItems { get; set; } = new List<DatasetItem>();

    public virtual User? CreatedByUser { get; set; }
}

