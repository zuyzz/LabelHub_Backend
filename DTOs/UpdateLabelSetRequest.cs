namespace DataLabel_Project_BE.DTOs;

public class UpdateLabelSetRequest
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public Guid? GuidelineId { get; set; }
}
