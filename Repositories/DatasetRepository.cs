using DataLabel_Project_BE.Data;
using DataLabel_Project_BE.Models;
using Microsoft.EntityFrameworkCore;

namespace DataLabel_Project_BE.Repositories;

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

    public async Task AddAnnotationTasksAsync(IEnumerable<AnnotationTask> tasks)
    {
        await _context.AnnotationTasks.AddRangeAsync(tasks);
    }

    public async Task<bool> ProjectExistsAsync(Guid projectId)
    {
        return await _context.Projects.AnyAsync(p => p.ProjectId == projectId);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}