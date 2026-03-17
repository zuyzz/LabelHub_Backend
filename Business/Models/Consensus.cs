using System;

namespace DataLabelProject.Business.Models;

public partial class Consensus
{
    public Guid ConsensusId { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid DatasetItemId { get; set; }

    public string Payload { get; set; } = null!;

    public virtual DatasetItem DatasetItem { get; set; } = null!;

    public virtual Review Review { get; set; } = null!;
}
