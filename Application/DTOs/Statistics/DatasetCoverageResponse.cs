namespace DataLabelProject.Application.DTOs.Statistics;

public class DatasetCoverageResponse
{
    public int DatasetItems { get; set; }
    public int ItemsAnnotated { get; set; }
    public int ItemsConsensus { get; set; }
    public double CoveragePercent { get; set; }
}
