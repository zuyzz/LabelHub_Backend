using DataLabelProject.Application.DTOs.Common;
using DataLabelProject.Application.DTOs.Consensus;
using DataLabelProject.Business.Models;

namespace DataLabelProject.Data.Repositories.Abstractions;

public interface IConsensusRepository
{
	Task<Consensus> CreateAsync(Consensus consensus);
	Task UpdateAsync(Consensus consensus);
	Task<Consensus?> GetByIdAsync(Guid consensusId);
	Task<Consensus?> GetByReviewIdAsync(Guid reviewId);
	Task<IEnumerable<Consensus>> GetByDatasetItemIdAsync(Guid taskId);
	Task<PagedResult<Consensus>> GetConsensusesAsync(ConsensusQueryParameters @params);
}
