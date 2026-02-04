using DataLabel_Project_BE.DTOs;
using DataLabel_Project_BE.Models;
using DataLabel_Project_BE.Repositories;
using DataLabel_Project_BE.Services.Uploads;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace DataLabel_Project_BE.Services;

public class DatasetService : IDatasetService
{
    private readonly IDatasetRepository _repo;
    private readonly IProjectRepository _projectRepo;
    private readonly IEnumerable<IFileUploadStrategy> _strategies;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly Services.Storage.IFileStorage _storage;

    public DatasetService(IDatasetRepository repo, IProjectRepository projectRepo, IEnumerable<IFileUploadStrategy> strategies, IHttpContextAccessor httpContextAccessor, Services.Storage.IFileStorage storage)
    {
        _repo = repo;
        _projectRepo = projectRepo;
        _strategies = strategies;
        _httpContextAccessor = httpContextAccessor;
        _storage = storage;
    }

    public async Task<DatasetImportResponse> ImportDatasetAsync(Guid projectId, DatasetImportRequest request)
    {
        Console.WriteLine("STEP 1: ImportDatasetAsync entered");
        // Validate project exists
        if (!await _repo.ProjectExistsAsync(projectId)) throw new KeyNotFoundException("Project not found");

        // Validate user allowed: must be authenticated and (manager/admin) or project member
        var user = _httpContextAccessor.HttpContext?.User;
        var userIdClaim = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId)) throw new UnauthorizedAccessException();

        var isAdminOrManager = user?.IsInRole("admin") == true || user?.IsInRole("manager") == true;
        var isMember = await _projectRepo.ProjectMemberExistsAsync(projectId, userId);
        if (!isAdminOrManager && !isMember) throw new UnauthorizedAccessException();

        // Ensure project folder exists in storage: project-{projectId}
        await _storage.EnsureFolderAsync($"project-{projectId}");

        // Determine the file to process: either the uploaded file or a server-local path
        var file = request.File;

        if (file == null) throw new InvalidOperationException("No file to process");

        // choose strategy
        var strategy = _strategies.FirstOrDefault(s => s.CanHandle(file));
        if (strategy == null) throw new InvalidOperationException("No upload strategy can handle this file type");

        var datasetName = string.IsNullOrWhiteSpace(request.Name) ? Path.GetFileNameWithoutExtension(file.FileName ?? "dataset") + "-import" : request.Name!;

        Console.WriteLine("STEP 2: About to upload file");
        var result = await strategy.ProcessAsync(file, projectId, datasetName);
        Console.WriteLine("STEP 3: Upload finished");

        // Create dataset record
        var dataset = new Dataset
        {
            DatasetId = Guid.NewGuid(),
            ProjectId = projectId,
            Name = datasetName,
            Description = request.Description,
            StorageUri = result.StoragePrefix,
            VersionNumber = 1,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        await _repo.CreateDatasetAsync(dataset);

        // Create annotation tasks for each item
        var tasks = result.Items.Select(item => new AnnotationTask
        {
            TaskId = Guid.NewGuid(),
            DatasetId = dataset.DatasetId,
            ScopeUri = item.StorageUri,
            Status = "unstarted",
            CreatedAt = DateTime.UtcNow
        }).ToList();

        if (tasks.Any()) await _repo.AddAnnotationTasksAsync(tasks);

        await _repo.SaveChangesAsync();

        Console.WriteLine($"ImportDatasetAsync completed for projectId: {projectId}");
        return new DatasetImportResponse(dataset.DatasetId, dataset.Name, dataset.Description, dataset.StorageUri, tasks.Count);
    }

    private static string GetContentTypeByExtension(string? ext)
    {
        if (string.IsNullOrWhiteSpace(ext)) return "application/octet-stream";
        return ext.ToLowerInvariant() switch
        {
            ".png" => "image/png",
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".json" => "application/json",
            ".xml" => "application/xml",
            ".zip" => "application/zip",
            ".rar" => "application/vnd.rar",
            _ => "application/octet-stream",
        };
    }
}