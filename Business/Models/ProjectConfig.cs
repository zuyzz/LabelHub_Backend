using System;

namespace DataLabelProject.Business.Models;

public partial class ProjectConfig
{
    public Guid ProjectConfigId { get; set; }

    public double DefaultAnnotateDeadlineInterval { get; set; } = 28800;

    public double DefaultReviewDeadlineInterval { get; set; } = 18000;

    public Guid ProjectId { get; set; }

    public double AgreementThreshold { get; set; } = 0.8;

    public int AnnotationsPerSample { get; set; } = 3;

    public virtual Project Project { get; set; } = null!;
}
