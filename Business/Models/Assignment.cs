using System;
using System.Collections.Generic;
using DataLabelProject.Business.Models.Enums;

namespace DataLabelProject.Business.Models;

public partial class Assignment
{
    public Guid AssignmentId { get; set; }

    public Guid TaskId { get; set; }

    public Guid AssignedTo { get; set; }

    public Guid AssignedBy { get; set; }

    public DateTime AssignedAt { get; set; }

    public DateTime? StartedAt { get; set; }

    public double TimeLimitMinutes { get; set; }

    public virtual User AssignedByUser { get; set; } = null!;

    public virtual LabelingTask AssignmentTask { get; set; } = null!;

    public virtual User AssignmentUser { get; set; } = null!;
}
