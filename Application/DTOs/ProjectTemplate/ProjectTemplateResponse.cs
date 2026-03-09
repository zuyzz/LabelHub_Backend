namespace DataLabelProject.Application.DTOs.ProjectTemplate
{
    public record ProjectTemplateResponse
    {
        public Guid TemplateId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string MediaType { get; set; } = "image";
    }
}