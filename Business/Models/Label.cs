using System;
using System.Collections.Generic;

namespace DataLabelProject.Business.Models;

public partial class Label
{
    public Guid LabelId { get; set; }

    public Guid CategoryId { get; set; }

    public string Name { get; set; } = null!;

    public bool IsActive { get; set; } = true;

    public Guid CreatedBy { get; set; }

    public virtual Category LabelCategory { get; set; } = null!;

    public virtual User LabelCreatedByUser { get; set; } = null!;
}
