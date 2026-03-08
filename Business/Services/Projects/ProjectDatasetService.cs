using DataLabelProject.Application.DTOs.Datasets;
using DataLabelProject.Business.Models;
using DataLabelProject.Data.Repositories.Abstractions;
using System.Security.Claims;

namespace DataLabelProject.Business.Services.Projects;

public class ProjectDatasetService : IProjectDatasetService
{
    private readonly IProjectDatasetRepository _repo;
    private readonly IProjectRepository _projectRepo;
    private readonly IDatasetRepository _datasetRepo;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ProjectDatasetService(
        IProjectDatasetRepository repo,
        IProjectRepository projectRepo,
        IDatasetRepository datasetRepo,
        IHttpContextAccessor httpContextAccessor)
    {
        _repo = repo;
        _projectRepo = projectRepo;
        _datasetRepo = datasetRepo;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<AttachDatasetResponse> AttachDatasetAsync(Guid datasetId, Guid projectId)
    {
        // Get current user ID
        var user = _httpContextAccessor.HttpContext?.User;
        var userIdClaim = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedAccessException("User not authenticated");

        // Get dataset and verify current user owns it
        var dataset = await _datasetRepo.GetDatasetByIdAsync(datasetId);
        if (dataset == null)
            throw new KeyNotFoundException($"Dataset with ID {datasetId} not found");

        if (dataset.CreatedBy != userId)
            throw new UnauthorizedAccessException("You can only attach datasets you created");

        // Get project and verify current user owns it
        var project = await _projectRepo.GetByIdAsync(projectId);
        if (project == null)
            throw new KeyNotFoundException($"Project with ID {projectId} not found");

        if (project.CreatedBy != userId)
            throw new UnauthorizedAccessException("You can only attach datasets to projects you created");

        // Check if already attached
        var isAttached = await _repo.IsDatasetAttachedAsync(projectId, datasetId);
        if (isAttached)
            throw new InvalidOperationException("Dataset is already attached to this project");

        // Create the attachment
        var projectDataset = new ProjectDataset
        {
            ProjectId = projectId,
            DatasetId = datasetId,
            AttachedAt = DateTime.UtcNow,
            AttachedBy = userId
        };

        await _repo.AttachDatasetAsync(projectDataset);
        await _repo.SaveChangesAsync();

        return new AttachDatasetResponse(projectId, datasetId, projectDataset.AttachedAt, projectDataset.AttachedBy);
    }

    public async Task DetachDatasetAsync(Guid projectId, Guid datasetId)
    {
        // Get current user ID
        var user = _httpContextAccessor.HttpContext?.User;
        var userIdClaim = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedAccessException("User not authenticated");

        // Verify current user owns the project
        var project = await _projectRepo.GetByIdAsync(projectId);
        if (project == null)
            throw new KeyNotFoundException($"Project with ID {projectId} not found");

        if (project.CreatedBy != userId)
            throw new UnauthorizedAccessException("You can only detach datasets from projects you created");

        await _repo.DetachDatasetAsync(projectId, datasetId);
        await _repo.SaveChangesAsync();
    }

    public async Task<bool> IsDatasetAttachedAsync(Guid projectId, Guid datasetId)
    {
        return await _repo.IsDatasetAttachedAsync(projectId, datasetId);
    }

    public async Task<IEnumerable<DatasetResponse>> GetDatasetsByProjectAsync(Guid projectId)
    {
        var datasets = await _repo.GetDatasetsByProjectAsync(projectId);
        // map to response DTOs similar to DatasetService
        return datasets.Select(dataset => new DatasetResponse(
            dataset.DatasetId,
            dataset.Name,
            dataset.Description,
            dataset.CreatedAt,
            dataset.CreatedBy,
            dataset.DatasetItems?.Count ?? 0
        ));
    }
}
