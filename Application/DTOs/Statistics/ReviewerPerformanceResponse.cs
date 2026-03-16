namespace DataLabelProject.Application.DTOs.Statistics;

public class ReviewerPerformanceResponse
{
    public Guid ReviewerId { get; set; }
    public string DisplayName { get; set; } = null!;
    public int Reviews { get; set; }
    public int Approved { get; set; }
    public int Rejected { get; set; }
}
