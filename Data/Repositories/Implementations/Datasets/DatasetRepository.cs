using DataLabelProject.Data;
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

    public async Task<Dataset> CreateDatasetAsync(Dataset dataset)
    {
        await _context.Datasets.AddAsync(dataset);
        return dataset;
    }

    public async Task<Dataset?> GetDatasetByIdAsync(Guid datasetId)
    {
        return await _context.Datasets
            .Include(d => d.DatasetItems)
            .FirstOrDefaultAsync(d => d.DatasetId == datasetId);
    }

    public async Task<Dataset> UpdateDatasetAsync(Dataset dataset)
    {
        _context.Datasets.Update(dataset);
        return dataset;
    }

    public async Task DeleteDatasetAsync(Guid datasetId)
    {
        var dataset = await _context.Datasets
            .Include(d => d.DatasetItems)
            .FirstOrDefaultAsync(d => d.DatasetId == datasetId);

        if (dataset != null)
        {
            // Delete related annotation tasks
            var tasks = await _context.AnnotationTasks
                .Where(t => t.DatasetId == datasetId)
                .ToListAsync();

            _context.AnnotationTasks.RemoveRange(tasks);

            // Delete dataset items
            _context.DatasetItems.RemoveRange(dataset.DatasetItems);

            // Delete dataset
            _context.Datasets.Remove(dataset);
        }
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
