namespace DataLabelProject.Application.DTOs.Exports;

public record ExportJobResponse
{
    public Guid ExportId { get; set; }
    public Guid ProjectId { get; set; }
    public Guid InitiatorId { get; set; }
    public string Format { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string? FileUri { get; set; }
    public DateTime CreatedAt { get; set; }
}
