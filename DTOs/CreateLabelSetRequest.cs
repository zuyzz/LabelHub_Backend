namespace DataLabel_Project_BE.DTOs;

public class CreateLabelSetRequest
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public Guid? GuidelineId { get; set; }
}
