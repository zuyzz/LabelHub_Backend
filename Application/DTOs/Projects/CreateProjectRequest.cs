using System.ComponentModel.DataAnnotations;

namespace DataLabelProject.Application.DTOs.Projects
{
    public class CreateProjectRequest
    {
        [Required]
        public string Name { get;set; } = null!;
        public string? Description { get;set; }
        public Guid CategoryId { get;set; }
        public Guid TemplateId { get;set; }
    };
}