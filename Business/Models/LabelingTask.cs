using System;
using System.Collections.Generic;
using DataLabelProject.Business.Models.Enums;

namespace DataLabelProject.Business.Models;

public partial class LabelingTask
{
    public Guid TaskId { get; set; }

    public Guid DatasetItemId { get; set; }

    public Guid ProjectId { get; set; }

    public int RevisionCount { get; set; } = 0;

    public LabelingTaskStatus Status { get; set; } = LabelingTaskStatus.active;

    public virtual ICollection<Annotation> Annotations { get; set; } = new List<Annotation>();

    public virtual ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual DatasetItem LabelingTaskDatasetItem { get; set; } = null!;

    public virtual Project LabelingTaskProject { get; set; } = null!;
}
