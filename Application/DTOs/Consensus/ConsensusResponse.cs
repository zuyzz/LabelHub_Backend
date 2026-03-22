using DataLabelProject.Business.Models.Enums;

namespace DataLabelProject.Application.DTOs.Consensus;

public class ConsensusResponse
{
    public Guid ConsensusId { get; set; }
    public Guid DatasetItemId { get; set; }
    public object Payload { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public ReviewResult? Result { get; set; }
}
