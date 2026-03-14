namespace DataLabelProject.Application.DTOs.Statistics;

public class ProjectOverviewResponse
{
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = null!;
    public int DatasetItems { get; set; }
    public int TasksTotal { get; set; }
    public int TasksOpened { get; set; }
    public int TasksClosed { get; set; }
    public int TasksRemoved { get; set; }
    public int AnnotationsTotal { get; set; }
    public int ConsensusGenerated { get; set; }
    public double AgreementAverage { get; set; }
}
