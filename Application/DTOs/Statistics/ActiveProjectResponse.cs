namespace DataLabelProject.Application.DTOs.Statistics;

public class ActiveProjectResponse
{
    public Guid ProjectId { get; set; }
    public string Name { get; set; } = null!;
    public int AnnotationsToday { get; set; }
    public int ActiveAnnotators { get; set; }
}
