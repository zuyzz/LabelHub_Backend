using System;
using System.Collections.Generic;

namespace DataLabelProject.Business.Models;

public partial class SystemConfig
{
    public Guid SystemConfigId { get; set; }

    public int? AnnotateDeadlineConfig { get; set; }

    public int? ReviewDeadlineInterval { get; set; }
}
