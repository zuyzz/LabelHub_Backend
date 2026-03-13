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
		await _context.Set<ConsensusEntity>().AddAsync(consensus);
		await _context.SaveChangesAsync();
		return consensus;
	}

	public async Task UpdateAsync(ConsensusEntity consensus)
	{
		_context.Set<ConsensusEntity>().Update(consensus);
		await _context.SaveChangesAsync();
	}

	public async Task<ConsensusEntity?> GetByIdAsync(Guid consensusId)
	{
		return await _context.Set<ConsensusEntity>()
			.AsNoTracking()
			.FirstOrDefaultAsync(c => c.ConsensusId == consensusId);
	}

	public async Task<IEnumerable<ConsensusEntity>> GetByTaskIdAsync(Guid taskId)
	{
		return await _context.Set<ConsensusEntity>()
			.AsNoTracking()
			.Where(c => c.TaskId == taskId)
			.OrderByDescending(c => c.CreatedAt)
			.ToListAsync();
	}

	public async Task<PagedResult<ConsensusEntity>> GetConsensusesAsync(ConsensusQueryParameters @params)
	{
		var query = _context.Set<ConsensusEntity>()
			.AsNoTracking()
			.OrderByDescending(c => c.CreatedAt)
			.AsQueryable();

		if (@params.TaskId.HasValue)
			query = query.Where(c => c.TaskId == @params.TaskId.Value);

		if (@params.MinAgreementScore.HasValue)
			query = query.Where(c => c.AgreementScore >= @params.MinAgreementScore.Value);

		if (@params.MaxAgreementScore.HasValue)
			query = query.Where(c => c.AgreementScore <= @params.MaxAgreementScore.Value);

		var total = await query.CountAsync();
		var items = await query
			.Skip(@params.Offset)
			.Take(@params.PageSize)
			.ToListAsync();

		return new PagedResult<ConsensusEntity>
		{
			Items = items,
			TotalItems = total
		};
	}
}
