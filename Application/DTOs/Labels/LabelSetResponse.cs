namespace DataLabelProject.Application.DTOs.Labels;

public class LabelSetResponse
{
    public Guid LabelSetId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public Guid? ProjectId { get; set; }
    public DateTime CreatedAt { get; set; }
}
