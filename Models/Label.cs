using System;
using System.Collections.Generic;

namespace DataLabel_Project_BE.Models;

public partial class Label
{
    public Guid LabelId { get; set; }

    public Guid LabelSetId { get; set; }

    public string Name { get; set; } = null!;

    public bool IsActive { get; set; }

    public virtual LabelSet LabelLabelSet { get; set; } = null!;
}
