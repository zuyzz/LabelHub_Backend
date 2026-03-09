
namespace DataLabelProject.Application.DTOs.Projects
{
    public record UpdateProjectRequest
    {
        public string? Name { get;set; }
        public string? Description { get;set; }
        public bool? IsActive { get;set; }
    };   
}
