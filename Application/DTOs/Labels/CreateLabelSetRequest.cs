namespace DataLabelProject.Application.DTOs.Labels;

public class CreateLabelSetRequest
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public Guid? ProjectId { get; set; }
}
