using DataLabelProject.Business.Models.Enums;

namespace DataLabelProject.Application.DTOs.ProjectTemplate
{
    public class UpdateProjectTemplateRequest
    {
        public string? Name { get; set; } = string.Empty;
        public MediaType? MediaType { get; set; }
    }
}