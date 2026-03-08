using System;
using System.Collections.Generic;

namespace DataLabelProject.Business.Models;

public partial class Guideline
{
    public Guid GuidelineId { get; set; }

    public string Content { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public Guid? ProjectId { get; set; }

    public virtual Project? GuidelineProject { get; set; }
}
