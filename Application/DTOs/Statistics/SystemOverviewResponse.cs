namespace DataLabelProject.Application.DTOs.Statistics;

public class SystemOverviewResponse
{
    public int Users { get; set; }
    public int Projects { get; set; }
    public int Datasets { get; set; }
    public int DatasetItems { get; set; }
    public int Annotations { get; set; }
    public int ConsensusGenerated { get; set; }
}
