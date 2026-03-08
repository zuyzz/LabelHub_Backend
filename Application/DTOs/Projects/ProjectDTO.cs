using DataLabelProject.Application.DTOs.Categories;

namespace DataLabelProject.Application.DTOs.Projects
{
    public record ProjectCreateRequest(
        string Name,
        string? Description,
        Guid CategoryId,
        Guid TemplateId
    );

    public record ProjectUpdateRequest(
        string? Name,
        string? Description,
        bool? IsActive
    );

    public class ProjectResponse
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

    public class ProjectTemplateResponse
    {
        public Guid TemplateId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string MediaType { get; set; } = "image";
    }

    public record ProjectTemplateCreateRequest(
        string Name,
        string MediaType
    );

    public record ProjectTemplateUpdateRequest(
        string Name,
        string MediaType
    );
}
