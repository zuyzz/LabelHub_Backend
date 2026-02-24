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
        var datasetName = request.Name ?? "untitled-dataset";

        string storageUri = string.Empty;
        int fileCount = 0;

        // Process file if provided
        if (request.File != null)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                datasetName = Path.GetFileNameWithoutExtension(request.File.FileName ?? "dataset");

            // Choose strategy based on file type
            var strategy = _strategies.FirstOrDefault(s => s.CanHandle(request.File));
            if (strategy == null)
                throw new InvalidOperationException("File type is not supported. Only image files or archives containing image files are accepted.");

            // Process file upload
            var result = await strategy.ProcessAsync(request.File, datasetId, datasetName);
            storageUri = result.StoragePrefix;
            fileCount = result.Items.Count();
        }

        // Create dataset record
        var dataset = new Dataset
        {
            DatasetId = datasetId,
            Name = datasetName,
            Description = request.Description,
            StorageUri = storageUri,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        await _repo.CreateDatasetAsync(dataset);
        await _repo.SaveChangesAsync();

        return new CreateDatasetResponse(
            datasetId,
            datasetName,
            dataset.Description,
            storageUri,
            fileCount);
    }

    public async Task<UpdateDatasetResponse> UpdateDatasetAsync(Guid datasetId, UpdateDatasetRequest request)
    {
        var dataset = await _repo.GetDatasetByIdAsync(datasetId);
        if (dataset == null)
            throw new KeyNotFoundException($"Dataset with ID {datasetId} not found");

        if (!string.IsNullOrWhiteSpace(request.Name))
            dataset.Name = request.Name;

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
            dataset.StorageUri ?? string.Empty,
            dataset.CreatedAt,
            dataset.CreatedBy,
            dataset.DatasetItems?.Count ?? 0);
    }
}
