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

    public IQueryable<Dataset> Query()
    {
        return _context.Datasets;
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
