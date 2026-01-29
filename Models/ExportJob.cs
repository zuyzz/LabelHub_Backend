using System;
using System.Collections.Generic;

namespace DataLabel_Project_BE.Models;

public partial class ExportJob
{
    public Guid ExportId { get; set; }

    public Guid InitiatorId { get; set; }

    public Guid? TargetProjectId { get; set; }

    public string Format { get; set; } = null!;

    public string Status { get; set; } = null!;

    public string? FileUri { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User ExportInitiator { get; set; } = null!;

    public virtual Project? TargetProject { get; set; }
}
