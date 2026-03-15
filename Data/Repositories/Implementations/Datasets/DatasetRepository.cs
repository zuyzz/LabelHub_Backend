using DataLabelProject.Application.DTOs.Datasets;
using DataLabelProject.Business.Models;
using DataLabelProject.Data.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace DataLabelProject.Data.Repositories.Implementations.Datasets;

public class DatasetRepository : IDatasetRepository
{
    private readonly AppDbContext _context;

    public DatasetRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<Dataset> Items, int TotalCount)> GetAllAsync(DatasetQueryParameters @params)
    {
        var query = _context.Datasets
            .AsNoTracking()
            .Include(d => d.DatasetItems)
            .OrderByDescending(d => d.CreatedAt)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(@params.Name))
            query = query.Where(d => EF.Functions.ILike(d.Name, $"%{@params.Name.Trim()}%"));
            
        if (!string.IsNullOrWhiteSpace(@params.Description))
            query = query.Where(d => EF.Functions.ILike(d.Description ?? "", $"%{@params.Description}%"));

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip(@params.Offset)
            .Take(@params.PageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<(IEnumerable<Dataset> Items, int TotalCount)> GetAllByCreatorAsync(Guid creatorId, DatasetQueryParameters @params)
    {
        var query = _context.Datasets
            .AsNoTracking()
            .Where(d => d.CreatedBy == creatorId)
            .Include(d => d.DatasetItems)
            .Include(d => d.DatasetProject)
            .OrderByDescending(d => d.CreatedAt)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(@params.Name))
            query = query.Where(d => EF.Functions.ILike(d.Name, $"%{@params.Name.Trim()}%"));

        if (!string.IsNullOrWhiteSpace(@params.Description))
            query = query.Where(d => EF.Functions.ILike(d.Description ?? "", $"%{@params.Description.Trim()}%"));

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip(@params.Offset)
            .Take(@params.PageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<Dataset?> GetByIdAsync(Guid id)
    {
        return await _context.Datasets
            .Include(d => d.DatasetItems)
            .Include(d => d.DatasetProject)
            .FirstOrDefaultAsync(d => d.DatasetId == id);
    }

    public async Task<Dataset?> GetByNameAndCreatorAsync(string name, Guid creatorId)
    {
        return await _context.Datasets
            .FirstOrDefaultAsync(d => d.Name == name && d.CreatedBy == creatorId);
    }

    public async Task CreateAsync(Dataset dataset)
    {
        await _context.Datasets.AddAsync(dataset);
    }

    public async Task UpdateAsync(Dataset dataset)
    {
        _context.Datasets.Update(dataset);
    }

    public async Task DeleteAsync(Dataset dataset)
    {
        _context.DatasetItems.RemoveRange(dataset.DatasetItems);
        _context.Datasets.Remove(dataset);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
