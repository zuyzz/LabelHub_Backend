namespace DataLabelProject.Application.DTOs.Consensus;

public class ConsensusDto
{
    public Guid ConsensusId { get; set; }
    public Guid TaskId { get; set; }
    public object Payload { get; set; } = null!;
    public double AgreementScore { get; set; }
    public DateTime CreatedAt { get; set; }
}
