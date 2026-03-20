using DataLabelProject.Application.DTOs.Common;
using DataLabelProject.Application.DTOs.Consensus;
using DataLabelProject.Data.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;
using ConsensusEntity = DataLabelProject.Business.Models.Consensus;

namespace DataLabelProject.Data.Repositories.Implementations.Consensus;

public class ConsensusRepository : IConsensusRepository
{
	private readonly AppDbContext _context;

	public ConsensusRepository(AppDbContext context)
	{
		_context = context;
	}

	public async Task<ConsensusEntity> CreateAsync(ConsensusEntity consensus)
	{
		await _context.Consensuses.AddAsync(consensus);
		await _context.SaveChangesAsync();
		return consensus;
	}

	public async Task UpdateAsync(ConsensusEntity consensus)
	{
		_context.Consensuses.Update(consensus);
		await _context.SaveChangesAsync();
	}

	public async Task<ConsensusEntity?> GetByIdAsync(Guid consensusId)
	{
		return await _context.Consensuses
			.FirstOrDefaultAsync(c => c.ConsensusId == consensusId);
	}

	public async Task<ConsensusEntity?> GetByReviewIdAsync(Guid reviewId)
	{
		return await _context.Consensuses
			.Include(c => c.Review)
			.FirstOrDefaultAsync(c => c.Review.ReviewId == reviewId);
	}

	public async Task<IEnumerable<ConsensusEntity>> GetByDatasetItemIdAsync(Guid datasetItemId)
	{
		return await _context.Consensuses
			.AsNoTracking()
			.Where(c => c.DatasetItemId == datasetItemId)
			.OrderByDescending(c => c.CreatedAt)
			.ToListAsync();
	}

	public async Task<PagedResponse<ConsensusEntity>> GetConsensusesAsync(ConsensusQueryParameters @params)
	{
		var query = _context.Consensuses
			.AsNoTracking()
			.OrderByDescending(c => c.CreatedAt)
			.AsQueryable();

		// fix

		// if (@params.TaskId.HasValue)
		// 	query = query.Where(c => c.TaskId == @params.TaskId.Value);

		// if (@params.MinAgreementScore.HasValue)
		// 	query = query.Where(c => c.AgreementScore >= @params.MinAgreementScore.Value);

		// if (@params.MaxAgreementScore.HasValue)
		// 	query = query.Where(c => c.AgreementScore <= @params.MaxAgreementScore.Value);

		var total = await query.CountAsync();
		var items = await query
			.Skip(@params.Offset)
			.Take(@params.PageSize)
			.ToListAsync();

		return new PagedResponse<ConsensusEntity>
		{
			Items = items,
			TotalItems = total
		};
	}
}
