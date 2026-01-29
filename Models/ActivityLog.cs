using System;
using System.Collections.Generic;

namespace DataLabel_Project_BE.Models;

public partial class ActivityLog
{
    public Guid ActivityLogId { get; set; }

    public Guid ProjectId { get; set; }

    public Guid? UserId { get; set; }

    public string EventType { get; set; } = null!;

    public string? TargetEntity { get; set; }

    public Guid? TargetId { get; set; }

    public string? Details { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Project Project { get; set; } = null!;

    public virtual User? ActivityLogUser { get; set; }
}
