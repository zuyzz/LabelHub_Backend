using System;
using System.Collections.Generic;

namespace DataLabel_Project_BE.Models;

public partial class SystemConfig
{
    public Guid SystemConfigId { get; set; }

    public int? AnnotateDeadlineConfig { get; set; }

    public int? ReviewDeadlineInterval { get; set; }
}
