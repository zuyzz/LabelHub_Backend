using DataLabelProject.Application.DTOs.Common;
using DataLabelProject.Business.Models.Enums;

namespace DataLabelProject.Application.DTOs.Annotations;

public class AnnotationQueryParameters : PaginationParameters
{
    public AnnotationStatus? Status { get; set; }
}
