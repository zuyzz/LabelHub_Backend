using System;
using System.Collections.Generic;

namespace DataLabelProject.Business.Models;

public partial class DatasetItem
{
    public Guid ItemId { get; set; }

    public Guid DatasetId { get; set; }

    public string MediaType { get; set; } = null!;

    public string StorageUri { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual Dataset ItemDataset { get; set; } = null!;

    public virtual ICollection<ProjectDatasetItem> ProjectDatasetItems { get; set; } = new List<ProjectDatasetItem>();
}
