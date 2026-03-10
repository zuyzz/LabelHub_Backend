using System;
using System.Collections.Generic;
using DataLabelProject.Business.Models.Enums;

namespace DataLabelProject.Business.Models;

public partial class ExportJob
{
    public Guid ExportId { get; set; }

    public Guid InitiatorId { get; set; }

    public Guid ProjectId { get; set; }

    public string Format { get; set; } = null!;

    public ExportJobStatus Status { get; set; }

    public string? FileUri { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User ExportInitiator { get; set; } = null!;

    public virtual Project TargetProject { get; set; } = null!;
}
