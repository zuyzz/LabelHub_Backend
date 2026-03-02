using DataLabelProject.Business.Models;
using DataLabelProject.Data;
using DataLabelProject.Data.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace DataLabelProject.Data.Repositories.Implementations.Projects;

public class ProjectDatasetRepository : IProjectDatasetRepository
{
    private readonly AppDbContext _context;

    public ProjectDatasetRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ProjectDataset> AttachDatasetAsync(ProjectDataset projectDataset)
    {
        await _context.ProjectDatasets.AddAsync(projectDataset);
        return projectDataset;
    }

    public async Task DetachDatasetAsync(Guid projectId, Guid datasetId)
    {
        var projectDataset = await _context.ProjectDatasets
            .FirstOrDefaultAsync(pd => pd.ProjectId == projectId && pd.DatasetId == datasetId);

        if (projectDataset != null)
        {
            _context.ProjectDatasets.Remove(projectDataset);
        }
    }

    public async Task<bool> IsDatasetAttachedAsync(Guid projectId, Guid datasetId)
    {
        return await _context.ProjectDatasets
            .AnyAsync(pd => pd.ProjectId == projectId && pd.DatasetId == datasetId);
    }

    public async Task<IEnumerable<Dataset>> GetDatasetsByProjectAsync(Guid projectId)
    {
        return await _context.ProjectDatasets
            .Where(pd => pd.ProjectId == projectId)
            .Include(pd => pd.Dataset)
            .Select(pd => pd.Dataset)
            .ToListAsync();
    }

    public async Task<IEnumerable<Project>> GetProjectsByDatasetAsync(Guid datasetId)
    {
        return await _context.ProjectDatasets
            .Where(pd => pd.DatasetId == datasetId)
            .Include(pd => pd.Project)
            .Select(pd => pd.Project)
            .ToListAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
