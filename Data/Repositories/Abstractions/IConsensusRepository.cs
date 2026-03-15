using DataLabelProject.Application.DTOs.Common;
using DataLabelProject.Application.DTOs.Consensus;
using ConsensusEntity = DataLabelProject.Business.Models.Consensus;

namespace DataLabelProject.Data.Repositories.Abstractions;

public interface IConsensusRepository
{
	Task<ConsensusEntity> CreateAsync(ConsensusEntity consensus);
	Task UpdateAsync(ConsensusEntity consensus);
	Task<ConsensusEntity?> GetByIdAsync(Guid consensusId);
	Task<IEnumerable<ConsensusEntity>> GetByDatasetItemIdAsync(Guid taskId);
	Task<PagedResult<ConsensusEntity>> GetConsensusesAsync(ConsensusQueryParameters @params);
}
