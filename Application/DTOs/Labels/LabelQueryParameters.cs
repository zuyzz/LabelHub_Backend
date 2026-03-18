using DataLabelProject.Application.DTOs.Common;

namespace DataLabelProject.Application.DTOs.Labels
{
    public class LabelQueryParameters : PaginationParameters
    {
        public string? Name { get; set; }
        public bool? IsActive { get; set; }
    }
}