using System;
using System.Collections.Generic;

namespace DataLabelProject.Business.Models;

public partial class LabelingTask
{
    public Guid TaskId { get; set; }

    public Guid DatasetItemId { get; set; }

    public Guid ProjectId { get; set; }

    public virtual ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();

    public virtual Project LabelingTaskProject { get; set; } = null!;
}
