using DataLabelProject.Application.DTOs.Common;

namespace DataLabelProject.Application.DTOs.Categories
{
    public class CategoryQueryParameters : PaginationParameters
    {
        public bool? IsActive { get; set; }
    }
}