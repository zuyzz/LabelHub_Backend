using DataLabelProject.Application.DTOs.Datasets;
using DataLabelProject.Business.Models;
using DataLabelProject.Data.Repositories.Abstractions;
using DataLabelProject.Business.Services.FileUpload;
using System.Security.Claims;

namespace DataLabelProject.Business.Services.Datasets;

public class DatasetService : IDatasetService
{
    private readonly IDatasetRepository _repo;
    private readonly IEnumerable<IFileUploadStrategy> _strategies;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly Storage.IFileStorage _storage;

    public DatasetService(
        IDatasetRepository repo,
        IEnumerable<IFileUploadStrategy> strategies,
        IHttpContextAccessor httpContextAccessor,
        Storage.IFileStorage storage)
    {
        _repo = repo;
        _strategies = strategies;
        _httpContextAccessor = httpContextAccessor;
        _storage = storage;
    }

    public async Task<CreateDatasetResponse> CreateDatasetAsync(CreateDatasetRequest request)
    {
        // Get current user ID
        var user = _httpContextAccessor.HttpContext?.User;
        var userIdClaim = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedAccessException("User not authenticated");
        // Generate dataset ID
        var datasetId = Guid.NewGuid();
        var datasetName = request.Name?.Trim() ?? throw new InvalidOperationException("Name is required");

        // If user is manager, enforce uniqueness per creator
        if (user?.IsInRole("manager") == true)
        {
            var exists = await _repo.GetByNameAndCreatorAsync(datasetName, userId);
            if (exists != null)
                throw new InvalidOperationException("You already have a dataset with the same name");
        }

        var dataset = new Dataset
        {
            DatasetId = datasetId,
            Name = datasetName,
            Description = request.Description,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        await _repo.CreateDatasetAsync(dataset);
        await _repo.SaveChangesAsync();

        return new CreateDatasetResponse(datasetId, datasetName, dataset.Description, 0);
    }

    public async Task<UpdateDatasetResponse> UpdateDatasetAsync(Guid datasetId, UpdateDatasetRequest request)
    {
        var dataset = await _repo.GetDatasetByIdAsync(datasetId);
        if (dataset == null)
            throw new KeyNotFoundException($"Dataset with ID {datasetId} not found");
        var user = _httpContextAccessor.HttpContext?.User;
        var userIdClaim = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        Guid.TryParse(userIdClaim, out var userId);

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            var newName = request.Name!.Trim();
            if (user?.IsInRole("manager") == true)
            {
                var existing = await _repo.GetByNameAndCreatorAsync(newName, userId);
                if (existing != null && existing.DatasetId != datasetId)
                    throw new InvalidOperationException("You already have a dataset with the same name");
            }

            dataset.Name = newName;
        }

        if (request.Description != null)
            dataset.Description = request.Description;

        await _repo.UpdateDatasetAsync(dataset);
        await _repo.SaveChangesAsync();

        return new UpdateDatasetResponse(datasetId, dataset.Name, dataset.Description);
    }

    public async Task DeleteDatasetAsync(Guid datasetId)
    {
        var dataset = await _repo.GetDatasetByIdAsync(datasetId);
        if (dataset == null)
            throw new KeyNotFoundException($"Dataset with ID {datasetId} not found");

        // delete objects in storage using predictable prefix
        var storagePrefix = $"datasets/{datasetId}/";
        await _storage.DeleteFolderAsync(storagePrefix);

        await _repo.DeleteDatasetAsync(datasetId);
        await _repo.SaveChangesAsync();
    }

    public async Task<DatasetResponse> GetDatasetByIdAsync(Guid datasetId)
    {
        var dataset = await _repo.GetDatasetByIdAsync(datasetId);
        if (dataset == null)
            throw new KeyNotFoundException($"Dataset with ID {datasetId} not found");

        return new DatasetResponse(
            dataset.DatasetId,
            dataset.Name,
            dataset.Description,
            dataset.CreatedAt,
            dataset.CreatedBy,
            dataset.MediaType.ToString(),
            dataset.DatasetItems?.Count ?? 0
        );
    }

    public async Task<IEnumerable<DatasetResponse>> GetDatasetsAsync()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var userIdClaim = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedAccessException("User not authenticated");

        IEnumerable<Business.Models.Dataset> list;
        if (user?.IsInRole("admin") == true)
        {
            list = await _repo.GetAllDatasetsAsync();
        }
        else
        {
            list = await _repo.GetDatasetsByCreatorAsync(userId);
        }

        return list.Select(dataset => new DatasetResponse(
            dataset.DatasetId,
            dataset.Name,
            dataset.Description,
            dataset.CreatedAt,
            dataset.CreatedBy,
            dataset.MediaType.ToString(),
            dataset.DatasetItems?.Count ?? 0
        ));
    }
}
