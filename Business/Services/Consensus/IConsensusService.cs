using DataLabelProject.Application.DTOs.Common;
using DataLabelProject.Application.DTOs.Consensus;

namespace DataLabelProject.Business.Services.Consensus;

public interface IConsensusService
{
	Task<ConsensusDto> CreateConsensusAsync(Guid taskId, ConsensusCreateRequest request);
	Task<ConsensusDto?> GetConsensusByIdAsync(Guid consensusId);
	Task<PagedResponse<ConsensusDto>> GetConsensusesAsync(ConsensusQueryParameters @params);
}
