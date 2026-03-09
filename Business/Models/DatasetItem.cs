using System;
using System.Collections.Generic;

namespace DataLabelProject.Business.Models;

public partial class DatasetItem
{
    public Guid ItemId { get; set; }

    public Guid DatasetId { get; set; }

    public string MediaType { get; set; } = null!;

    public string StorageUri { get; set; } = null!;

    public string Metadata { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<LabelingTask> LabelingTasks { get; set; } = new List<LabelingTask>();

    public virtual Dataset ItemDataset { get; set; } = null!;
}
