using DataLabelProject.Application.DTOs.Common;

namespace DataLabelProject.Application.DTOs.Datasets
{
    public class DatasetQueryParameters : PaginationParameters
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public Guid? ProjectId { get; set; }
    }
}