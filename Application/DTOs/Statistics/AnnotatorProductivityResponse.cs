namespace DataLabelProject.Application.DTOs.Statistics;

public class AnnotatorProductivityResponse
{
    public Guid UserId { get; set; }
    public string DisplayName { get; set; } = null!;
    public int Annotations { get; set; }
    public int CompletedAssignments { get; set; }
    public double? AvgTimePerTaskMinutes { get; set; }
}
