using DataLabelProject.Application.DTOs.Common;
using DataLabelProject.Business.Models.Enums;

namespace DataLabelProject.Application.DTOs.Consensus;

public class ConsensusQueryParameters : PaginationParameters
{
    public ReviewResult? Result { get; set; }
}
