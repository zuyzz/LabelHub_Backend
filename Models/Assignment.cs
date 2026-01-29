using System;
using System.Collections.Generic;

namespace DataLabel_Project_BE.Models;

public partial class Assignment
{
    public Guid AssignmentId { get; set; }

    public Guid TaskId { get; set; }

    public Guid UserId { get; set; }

    public Guid AssignedBy { get; set; }

    public DateTime AssignedAt { get; set; }

    public virtual User AssignedByUser { get; set; } = null!;

    public virtual AnnotationTask AssignmentTask { get; set; } = null!;

    public virtual User AssignmentUser { get; set; } = null!;
}
