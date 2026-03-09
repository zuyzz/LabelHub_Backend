using System;

namespace DataLabelProject.Business.Models;

public partial class Consensus
{
    public Guid ConsensusId { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid TaskId { get; set; }

    public string Payload { get; set; } = null!;

    public double AgreementScore { get; set; }

    public virtual LabelingTask LabelingTask { get; set; } = null!;
}
