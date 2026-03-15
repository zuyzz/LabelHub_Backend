using System;
using System.Collections.Generic;
using DataLabelProject.Business.Models.Enums;

namespace DataLabelProject.Business.Models;

public partial class LabelingTaskItem
{
    public Guid TaskItemId { get; set; }

    public Guid? TaskId { get; set; }

    public Guid ProjectId { get; set; }

    public Guid DatasetItemId { get; set; }

    public int RevisionCount { get; set; } = 0;

    public LabelingTaskItemStatus Status { get; set; } = LabelingTaskItemStatus.Unassigned;

    public virtual LabelingTask? Task { get; set; } = null!;

    public virtual Project Project { get; set; } = null!;
    
    public virtual DatasetItem DatasetItem { get; set; } = null!;

    public virtual Annotation Annotation { get; set; } = null!;

    public virtual Review Review { get; set; } = null!;
}
