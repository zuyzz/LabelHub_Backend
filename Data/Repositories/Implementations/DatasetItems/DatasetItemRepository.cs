using DataLabelProject.Data;
using DataLabelProject.Business.Models;
using DataLabelProject.Data.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace DataLabelProject.Data.Repositories.Implementations.DatasetItems;

public class DatasetItemRepository : IDatasetItemRepository
{
    private readonly AppDbContext _context;

    public DatasetItemRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<DatasetItem>> GetDatasetItemsAsync(Guid datasetId)
    {
        return await _context.DatasetItems
            .Where(i => i.DatasetId == datasetId)
            .ToListAsync();
    }

    public async Task<DatasetItem?> GetDatasetItemByIdAsync(Guid itemId)
    {
        return await _context.DatasetItems
            .FirstOrDefaultAsync(i => i.ItemId == itemId);
    }

    public async Task CreateDatasetItemAsync(DatasetItem item)
    {
        await _context.DatasetItems.AddAsync(item);
    }

    public async Task DeleteDatasetItemAsync(Guid itemId)
    {
        var item = await GetDatasetItemByIdAsync(itemId);
        if (item != null)
        {
            _context.DatasetItems.Remove(item);
        }
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}

