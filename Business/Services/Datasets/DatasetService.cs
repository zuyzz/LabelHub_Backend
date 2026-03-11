using DataLabelProject.Application.DTOs.Datasets;
using DataLabelProject.Business.Models;
using DataLabelProject.Data.Repositories.Abstractions;
using DataLabelProject.Business.Services.FileUpload;
using DataLabelProject.Business.Services.Storage;
using DataLabelProject.Business.Services.Users;
using DataLabelProject.Application.DTOs.Common;

namespace DataLabelProject.Business.Services.Datasets;

public class DatasetService : IDatasetService
{
    private readonly IDatasetRepository _datasetRepository;
    private readonly IDatasetItemRepository _datasetItemRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectDatasetRepository _projectDatasetRepository;
    private readonly ILabelingTaskRepository _taskRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileStorage _fileStorage;

    public DatasetService(
        IDatasetRepository datasetRepository,
        IDatasetItemRepository datasetItemRepository,
        IProjectRepository projectRepository,
        IProjectDatasetRepository projectDatasetRepository,
        ILabelingTaskRepository taskRepository,
        ICurrentUserService currentUserService,
        IFileStorage fileStorage)
    {
        _datasetRepository = datasetRepository;
        _datasetItemRepository = datasetItemRepository;
        _projectRepository = projectRepository;
        _projectDatasetRepository = projectDatasetRepository;
        _taskRepository = taskRepository;
        _currentUserService = currentUserService;
        _fileStorage = fileStorage;
    }

    public async Task<DatasetResponse> CreateDataset(CreateDatasetRequest request)
    {
        var currentUserId = _currentUserService.UserId!.Value;
        var datasetId = Guid.NewGuid();
        var datasetName = request.Name.Trim();

        var exists = await _datasetRepository.GetByNameAndCreatorAsync(datasetName, currentUserId);
        if (exists != null)
            throw new InvalidOperationException("You already have a dataset with the same name");

        var dataset = new Dataset
        {
            DatasetId = datasetId,
            Name = datasetName,
            Description = request.Description,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = currentUserId
        };

        await _datasetRepository.CreateAsync(dataset);
        await _datasetRepository.SaveChangesAsync();

        return MapToResponse(dataset);
    }

    public async Task<DatasetResponse> UpdateDataset(Guid id, UpdateDatasetRequest request)
    {
        var dataset = await _datasetRepository.GetByIdAsync(id);
        if (dataset == null)
            throw new KeyNotFoundException($"Dataset with ID {id} not found");

        var currentUserId = _currentUserService.UserId!.Value;

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            var newName = request.Name!.Trim();
            var existing = await _datasetRepository.GetByNameAndCreatorAsync(newName, currentUserId);
            if (existing != null && existing.DatasetId != id)
                throw new InvalidOperationException("You already have a dataset with the same name");

            dataset.Name = newName;
        }

        if (!string.IsNullOrWhiteSpace(request.Description))
            dataset.Description = request.Description;

        await _datasetRepository.UpdateAsync(dataset);
        await _datasetRepository.SaveChangesAsync();

        return MapToResponse(dataset);
    }

    public async Task<bool> DeleteDataset(Guid id)
    {
        var currentUserId = _currentUserService.UserId!.Value;
        var currentUserRole = _currentUserService.Roles;

        var dataset = await _datasetRepository.GetByIdAsync(id);
        if (dataset == null)
            throw new KeyNotFoundException($"Dataset with ID {id} not found");
        if (dataset.CreatedBy != currentUserId && currentUserRole.Contains("manager"))
            throw new UnauthorizedAccessException("Managers can only delete their own datasets");
        if (dataset.ProjectDatasets.Count > 0)
            throw new InvalidOperationException("Cannot delete dataset that is associated with projects. Please remove the dataset from those projects first.");

        var storagePrefix = $"datasets/{id}/";
        await _fileStorage.DeleteFolderAsync(storagePrefix);

        await _datasetRepository.DeleteAsync(dataset);
        await _datasetRepository.SaveChangesAsync();

        return true;
    }

    public async Task<DatasetResponse?> GetDatasetById(Guid id)
    {
        var dataset = await _datasetRepository.GetByIdAsync(id);
        if (dataset == null) return null;

        return MapToResponse(dataset);
    }

    public async Task<PagedResponse<DatasetResponse>> GetDatasets(DatasetQueryParameters @params)
    {
        var currentUserId = _currentUserService.UserId!.Value;
        var currentUserRole = _currentUserService.Roles;

        var (items, totalCount) = currentUserRole.Contains("admin") 
            ? await _datasetRepository.GetAllAsync(@params) 
            : await _datasetRepository.GetAllByCreatorAsync(currentUserId, @params);

        return new PagedResponse<DatasetResponse>
        {
            Items = items.Select(MapToResponse).ToList(),
            TotalItems = totalCount,
            Page = @params.Page,
            PageSize = @params.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)@params.PageSize)
        };
    }

    public async Task AddDatasetToProject(Guid datasetId, Guid projectId)
    {
        var currentUserId = _currentUserService.UserId!.Value;
        var currentUserRole = _currentUserService.Roles;

        var existing = await _projectDatasetRepository.GetByIdAsync(projectId, datasetId);
        if (existing != null)
            throw new InvalidOperationException("Dataset already exists in this project");

        var dataset = await _datasetRepository.GetByIdAsync(datasetId);
        if (dataset == null)
            throw new KeyNotFoundException("Dataset not found");

        var project = await _projectRepository.GetByIdAsync(projectId);
        if (project == null)
            throw new KeyNotFoundException("Project not found");

        if (currentUserRole.Contains("manager") &&
            (project.CreatedBy != currentUserId || dataset.CreatedBy != currentUserId))
        {
            throw new UnauthorizedAccessException(
                "Managers can only add their datasets to their own projects");
        }

        var projectDataset = new ProjectDataset
        {
            ProjectId = projectId,
            DatasetId = datasetId,
            AttachedBy = currentUserId
        };

        await _projectDatasetRepository.CreateAsync(projectDataset);
        await _projectDatasetRepository.SaveChangesAsync();

        var items = await _datasetItemRepository.GetAllByDatasetIdAsync(datasetId);
        var tasks = items.Select(i => new LabelingTask
        {
            TaskId = Guid.NewGuid(),
            ProjectId = projectId,
            DatasetItemId = i.ItemId
        });

        await _taskRepository.AddRangeAsync(tasks);
        await _taskRepository.SaveChangesAsync();
    }

    public async Task RemoveDatasetFromProject(Guid datasetId, Guid projectId)
    {
        var currentUserId = _currentUserService.UserId!.Value;
        var currentUserRoles = _currentUserService.Roles;

        var projectDataset = await _projectDatasetRepository.GetByIdAsync(projectId, datasetId);
        if (projectDataset == null)
            throw new KeyNotFoundException("Dataset does not exist in this project");

        if (currentUserRoles.Contains("manager") &&
            (projectDataset.Project.CreatedBy != currentUserId ||
            projectDataset.Dataset.CreatedBy != currentUserId))
        {
            throw new UnauthorizedAccessException(
                "Managers can only remove their datasets from their own projects");
        }

        var tasks = await _taskRepository.GetAllByDatasetIdAsync(datasetId);

        await _taskRepository.DeleteRangeAsync(tasks);
        await _taskRepository.SaveChangesAsync();

        await _projectDatasetRepository.DeleteAsync(projectDataset);
        await _projectDatasetRepository.SaveChangesAsync();
    }

    private DatasetResponse MapToResponse(Dataset dataset)
    {
        return new DatasetResponse
        {
            DatasetId = dataset.DatasetId,
            Name = dataset.Name,
            Description = dataset.Description,
            CreatedAt = dataset.CreatedAt,
            CreatedBy = dataset.CreatedBy,
            SampleCount = dataset.DatasetItems?.Count ?? 0
        };
    }
}
