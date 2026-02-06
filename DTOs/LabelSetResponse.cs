namespace DataLabel_Project_BE.DTOs;

public class LabelSetResponse
{
    public Guid LabelSetId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int VersionNumber { get; set; }
    public Guid? GuidelineId { get; set; }
    public DateTime CreatedAt { get; set; }
}
