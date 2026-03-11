using DataLabelProject.Business.Models;
using DataLabelProject.Data;
using DataLabelProject.Data.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace DataLabelProject.Data.Repositories.Implementations.Datasets;

public class ProjectDatasetRepository : IProjectDatasetRepository
{
    private readonly AppDbContext _context;

    public ProjectDatasetRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ProjectDataset>> GetProjectByDatasetAsync(Guid datasetId)
    {
        return await _context.ProjectDatasets
            .OrderByDescending(pd => pd.AttachedAt)
            .Include(pd => pd.Project)
            .Include(pd => pd.Dataset)
            .Where(pd => pd.DatasetId == datasetId)
            .ToListAsync();
    }

    public async Task<ProjectDataset?> GetByIdAsync(Guid projectId, Guid datasetId)
    {
        return await _context.ProjectDatasets
            .OrderByDescending(pd => pd.AttachedAt)
            .Include(pd => pd.Project)
            .Include(pd => pd.Dataset)
            .FirstOrDefaultAsync(pd => pd.ProjectId == projectId && pd.DatasetId == datasetId);
    }

    public async Task<ProjectDataset> CreateAsync(ProjectDataset projectDataset)
    {
        await _context.ProjectDatasets.AddAsync(projectDataset);
        return projectDataset;
    }

    public async Task DeleteAsync(ProjectDataset projectDataset)
    {
        _context.ProjectDatasets.Remove(projectDataset);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
