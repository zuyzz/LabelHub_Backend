using System;

namespace DataLabelProject.Business.Models;

public partial class ProjectDatasetItem
{
    public Guid ProjectId { get; set; }

    public Guid DatasetItemId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual DatasetItem DatasetItem { get; set; } = null!;

    public virtual Project Project { get; set; } = null!;
}
