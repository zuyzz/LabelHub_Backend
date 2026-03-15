using DataLabelProject.Application.DTOs.Common;

namespace DataLabelProject.Application.DTOs.Guidelines
{
    public class GuidelineQueryParameters : PaginationParameters
    {
        public Guid? ProjectId { get; set; }
        public string? Content { get; set; }
    }
}