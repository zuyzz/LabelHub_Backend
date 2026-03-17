using DataLabelProject.Application.DTOs.Common;
using DataLabelProject.Application.DTOs.Consensus;

namespace DataLabelProject.Business.Services.Consensus;

public interface IConsensusService
{
	Task<ConsensusResponse> CreateConsensusAsync(Guid taskId, ConsensusCreateRequest request);
	Task<ConsensusResponse?> GetConsensusByIdAsync(Guid consensusId);
	Task<PagedResponse<ConsensusResponse>> GetConsensusesAsync(ConsensusQueryParameters @params);
}
