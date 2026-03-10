using System.ComponentModel.DataAnnotations;

namespace DataLabelProject.Application.DTOs.Exports;

public class CreateExportRequest
{
    [Required]
    public string Format { get; set; } = null!;
}
