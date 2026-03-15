using DataLabelProject.Application.DTOs.Common;

namespace DataLabelProject.Application.DTOs.Labels
{
    public class LabelQueryParameters : PaginationParameters
    {
        public Guid? ProjectId { get; set; }
        public string? Name { get; set; }
        public Guid? CategoryId { get; set; }
    }
}