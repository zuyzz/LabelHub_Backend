using System;
using System.Collections.Generic;

namespace DataLabelProject.Business.Models;

public partial class Dataset
{
    public Guid DatasetId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid CreatedBy { get; set; }

    public bool IsActive { get; set; } = true;

    public virtual ICollection<DatasetItem> DatasetItems { get; set; } = new List<DatasetItem>();
    
    public virtual ICollection<ProjectDataset> ProjectDatasets { get; set; } = new List<ProjectDataset>();

    public virtual User CreatedByUser { get; set; } = null!;
}

