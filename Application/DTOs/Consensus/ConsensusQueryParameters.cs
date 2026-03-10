using DataLabelProject.Application.DTOs.Common;

namespace DataLabelProject.Application.DTOs.Consensus;

public class ConsensusQueryParameters : PaginationParameters
{
    public Guid? TaskId { get; set; }
    public double? MinAgreementScore { get; set; }
    public double? MaxAgreementScore { get; set; }
}
