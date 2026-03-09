using DataLabelProject.Application.DTOs.Categories;
using DataLabelProject.Application.DTOs.Common;
using DataLabelProject.Application.DTOs.Labels;
using DataLabelProject.Business.Models;
using DataLabelProject.Business.Services.Users;
using DataLabelProject.Data.Repositories.Abstractions;

namespace DataLabelProject.Business.Services.Labels;

public class LabelService : ILabelService
{
    private readonly ILabelRepository _labelRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectLabelRepository _projectLabelRepository;
    private readonly ICurrentUserService _currentUserService;

    public LabelService(
        ILabelRepository labelRepository,
        IProjectRepository projectRepository,
        IProjectLabelRepository projectLabelRepository,
        ICurrentUserService currentUserService)
    {
        _labelRepository = labelRepository;
        _projectRepository = projectRepository;
        _projectLabelRepository = projectLabelRepository;
        _currentUserService = currentUserService;
    }

    public async Task<PagedResponse<LabelResponse>> GetLabels(LabelQueryParameters @params)
    {
        var (items, totalCount) = await _labelRepository.GetAllAsync(@params);

        return new PagedResponse<LabelResponse>
        {
            Items = items.Select(MapToResponse),
            TotalItems = totalCount,
            Page = @params.Offset,
            PageSize = @params.PageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / @params.PageSize)
        };
    }

    public async Task<LabelResponse?> GetLabelById(Guid id)
    {
        var label = await _labelRepository.GetByIdAsync(id);
        if (label == null) return null;

        return MapToResponse(label);
    }

    public async Task<LabelResponse> CreateLabel(CreateLabelRequest request)
    {
        var name = request.Name.Trim();

        var (items, _) = await _labelRepository.GetAllAsync(new LabelQueryParameters
        {
            Name = name,
            CategoryId = request.CategoryId,
        });

        if (items.Any())
            throw new InvalidOperationException(
                "A label with the same name already exists in this category.");

        var label = new Label
        {
            LabelId = Guid.NewGuid(),
            CategoryId = request.CategoryId,
            Name = name,
            IsActive = true,
            CreatedBy = _currentUserService.UserId!.Value
        };

        await _labelRepository.CreateAsync(label);
        await _labelRepository.SaveChangesAsync();

        return MapToResponse(label);
    }

    public async Task<LabelResponse?> UpdateLabel(Guid id, UpdateLabelRequest request)
    {
        var label = await _labelRepository.GetByIdAsync(id);
        if (label == null) return null;

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            var name = request.Name.Trim();

            var (items, _) = await _labelRepository.GetAllAsync(new LabelQueryParameters
            {
                Name = name,
                CategoryId = label.CategoryId
            });

            var duplicate = items.Any(l => l.LabelId != id);

            if (duplicate)
                throw new InvalidOperationException(
                    "A label with the same name already exists in this category.");

            label.Name = name;
        }

        if (request.IsActive.HasValue)
            label.IsActive = request.IsActive.Value;

        await _labelRepository.UpdateAsync(label);
        await _labelRepository.SaveChangesAsync();

        return MapToResponse(label);
    }

    public async Task AddLabelToProject(Guid labelId, Guid projectId)
    {
        var currentUserId = _currentUserService.UserId!.Value;
        var currentUserRoles = _currentUserService.Roles;

        var existing = await _projectLabelRepository.GetByIdAsync(projectId, labelId);
        if (existing != null)
            throw new InvalidOperationException("Label already exists in this project");

        var project = await _projectRepository.GetByIdAsync(projectId);
        if (project == null)
            throw new KeyNotFoundException("Project not found");

        var label = await _labelRepository.GetByIdAsync(labelId);
        if (label == null)
            throw new KeyNotFoundException("Label not found");

        if (currentUserRoles.Contains("manager") &&
            project.CreatedBy != currentUserId)
        {
            throw new UnauthorizedAccessException(
                "Managers can only add labels to their own projects");
        }

        var projectLabel = new ProjectLabel
        {
            ProjectId = projectId,
            LabelId = labelId,
            AttachedBy = currentUserId
        };

        await _projectLabelRepository.CreateAsync(projectLabel);
        await _projectLabelRepository.SaveChangesAsync();
    }

    public async Task RemoveLabelFromProject(Guid labelId, Guid projectId)
    {
        var currentUserId = _currentUserService.UserId!.Value;
        var currentUserRoles = _currentUserService.Roles;

        var projectLabel = await _projectLabelRepository.GetByIdAsync(projectId, labelId);

        if (projectLabel == null)
            throw new KeyNotFoundException("Project or label not found");

        if (currentUserRoles.Contains("manager") &&
            projectLabel.Project.CreatedBy != currentUserId)
            throw new UnauthorizedAccessException(
                "Managers can only remove labels from their own projects");

        await _projectLabelRepository.DeleteAsync(projectLabel);
        await _projectLabelRepository.SaveChangesAsync();
    }

    private LabelResponse MapToResponse(Label label)
    {
        return new LabelResponse
        {
            LabelId = label.LabelId,
            Name = label.Name,
            IsActive = label.IsActive,
            CreatedBy = label.CreatedBy,
            Category = new CategoryResponse
            {
                CategoryId = label.CategoryId,
                Name = label.LabelCategory.Name
            }
        };
    }
}
