using System;

namespace DataLabelProject.Business.Models;

public partial class ProjectLabel
{
    public Guid ProjectId { get; set; }

    public Guid LabelId { get; set; }

    public DateTime AttachedAt { get; set; }

    public Guid AttachedBy { get; set; }

    public virtual Project Project { get; set; } = null!;

    public virtual Label Label { get; set; } = null!;

    public virtual User AttachedByUser { get; set; } = null!;
}
