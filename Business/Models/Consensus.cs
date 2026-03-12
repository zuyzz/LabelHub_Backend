using System;

namespace DataLabelProject.Business.Models;

public partial class Consensus
{
    public Guid ConsensusId { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid DatasetItemId { get; set; }

    public Guid ProjectId { get; set; }

    public string Payload { get; set; } = null!;

    public double AgreementScore { get; set; }

    public virtual DatasetItem ConsensusDatasetItem { get; set; } = null!;

    public virtual Project ConsensusProject { get; set; } = null!;
}
