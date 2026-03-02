using System;
using System.Collections.Generic;

namespace DataLabelProject.Business.Models;

public partial class ProjectDataset
{
    public Guid ProjectId { get; set; }

    public Guid DatasetId { get; set; }

    public DateTime AttachedAt { get; set; }

    public Guid? AttachedBy { get; set; }

    public virtual Project Project { get; set; } = null!;

    public virtual Dataset Dataset { get; set; } = null!;

    public virtual User? AttachedByUser { get; set; }
}
