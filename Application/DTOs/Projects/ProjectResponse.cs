using DataLabelProject.Application.DTOs.ProjectTemplate;
using DataLabelProject.Application.DTOs.Categories;

namespace DataLabelProject.Application.DTOs.Projects
{
    public record ProjectResponse
    {
        public Guid ProjectId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? CreatedBy { get; set; }
        public CategoryResponse Category { get; set; } = null!;
        public ProjectTemplateResponse Template { get; set; }  = null!;
    }
}