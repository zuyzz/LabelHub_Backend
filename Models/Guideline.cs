using System;
using System.Collections.Generic;

namespace DataLabel_Project_BE.Models;

public partial class Guideline
{
    public Guid GuidelineId { get; set; }

    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    public int Version { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<LabelSet> LabelSets { get; set; } = new List<LabelSet>();
}
