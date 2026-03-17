using DataLabelProject.Application.DTOs.Common;
using DataLabelProject.Business.Models.Enums;

namespace DataLabelProject.Application.DTOs.Reviews;

public class ReviewQueryParameters : PaginationParameters
{
    public ReviewResult? Result { get; set; }
}
