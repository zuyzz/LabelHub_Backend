namespace DataLabelProject.Application.DTOs.Guidelines;

public class GuidelineResponse
{
    public Guid GuidelineId { get; set; }
    public string Content { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}
