namespace DataLabel_Project_BE.DTOs.Guideline;

public class GuidelineResponse
{
    public Guid GuidelineId { get; set; }
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public int Version { get; set; }
    public DateTime CreatedAt { get; set; }
}
