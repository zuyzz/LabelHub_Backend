using DataLabelProject.Application.DTOs.Common;

namespace DataLabelProject.Application.DTOs.Guidelines
{
    public class GuidelineQueryParameters : PaginationParameters
    {
        public string? Content { get; set; }
    }
}