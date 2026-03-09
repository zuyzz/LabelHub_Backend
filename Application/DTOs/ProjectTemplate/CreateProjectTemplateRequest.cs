using DataLabelProject.Business.Models.Enums;

namespace DataLabelProject.Application.DTOs.ProjectTemplate
{
    public class CreateProjectTemplateRequest
    {
        public string Name { get; set; } = string.Empty;
        public MediaType MediaType { get; set; }
    }
}