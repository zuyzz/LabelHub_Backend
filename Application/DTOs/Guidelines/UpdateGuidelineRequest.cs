using System.ComponentModel.DataAnnotations;

namespace DataLabelProject.Application.DTOs.Guidelines
{
    public class UpdateGuidelineRequest
    {
        public string? Content { get; set; } = null!;
    }
}
