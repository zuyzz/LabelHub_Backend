using System;
using System.Collections.Generic;
using DataLabelProject.Business.Models.Enums;

namespace DataLabelProject.Business.Models;

public partial class LabelingTask
{
    public Guid TaskId { get; set; }

    public Guid ProjectId { get; set; }

    public LabelingTaskStatus Status { get; set; } = LabelingTaskStatus.Opened;

    public virtual ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();

    public virtual ICollection<LabelingTaskItem> TaskItems { get; set; } = new List<LabelingTaskItem>();

    public virtual Project LabelingTaskProject { get; set; } = null!;
}
