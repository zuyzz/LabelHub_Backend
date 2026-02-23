using System;
using System.Collections.Generic;

namespace DataLabelProject.Business.Models;

public partial class LabelSet
{
    public Guid LabelSetId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int VersionNumber { get; set; }

    public Guid? GuidelineId { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public virtual ICollection<Annotation> Annotations { get; set; } = new List<Annotation>();

    public virtual ICollection<Dataset> Datasets { get; set; } = new List<Dataset>();

    public virtual ICollection<Label> Labels { get; set; } = new List<Label>();

    public virtual User? CreatedByUser { get; set; }

    public virtual Guideline? LabelSetGuideline { get; set; }
}
