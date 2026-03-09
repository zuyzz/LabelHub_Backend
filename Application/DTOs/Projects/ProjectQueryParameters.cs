using DataLabelProject.Application.DTOs.Common;

namespace DataLabelProject.Application.DTOs.Projects
{
    public class ProjectQueryParameters : PaginationParameters
    {
        public string? Name { get; set; }

        public Guid? CategoryId { get; set; }

        public bool? IsActive { get; set; }
    }
}
