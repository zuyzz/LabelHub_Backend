using DataLabelProject.Application.DTOs.Projects;
using DataLabelProject.Application.DTOs.Common;
using DataLabelProject.Business.Models;
using DataLabelProject.Data.Repositories.Abstractions;
using DataLabelProject.Business.Services.Users;
using DataLabelProject.Business.Events.Abstraction;
using DataLabelProject.Business.Events.DomainEvents.Project;

namespace DataLabelProject.Business.Services.Projects;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectMemberRepository _projectMemberRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IProjectTemplateRepository _templateRepository;
    private readonly IProjectConfigRepository _configRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEventDispatcher _eventDispatcher;

    public ProjectService (
        IProjectRepository projectRepository,
        IProjectMemberRepository projectMemberRepository,
        ICategoryRepository categoryRepository,
        IProjectTemplateRepository templateRepository,
        IProjectConfigRepository configRepository,
        ICurrentUserService currentUserService,
        IEventDispatcher eventDispatcher)
    {
        _projectRepository = projectRepository;
        _projectMemberRepository = projectMemberRepository;
        _categoryRepository = categoryRepository;
        _templateRepository = templateRepository;
        _configRepository = configRepository;
        _currentUserService = currentUserService;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<PagedResponse<ProjectResponse>> GetProjects(ProjectQueryParameters @params)
    {
        var currentUserId = _currentUserService.UserId!.Value;
        var currentUserRole = _currentUserService.Roles;

        var (items, totalCount) = currentUserRole.Contains("admin") 
            ? await _projectRepository.GetAllAsync(@params)
            : await _projectRepository.GetAllByUserAsync(currentUserId, @params);

        var mapped = items.Select(MapToResponse).ToList();

        return new PagedResponse<ProjectResponse>
        {
            Items = mapped,
            TotalItems = totalCount,
            Page = @params.Page,
            PageSize = @params.PageSize,
        };
    }

    public async Task<ProjectResponse?> GetProjectById(Guid id)
    {
        var project = await _projectRepository.GetByIdAsync(id);
        return project == null ? null : MapToResponse(project);
    }

    public async Task<ProjectResponse> CreateProject(CreateProjectRequest request)
    {
        var category = await _categoryRepository.GetByIdAsync(request.CategoryId);
        if (category == null)
            throw new InvalidOperationException("Category not found");

        var template = await _templateRepository.GetByIdAsync(request.TemplateId);
        if (template == null)
            throw new InvalidOperationException("Template not found");

        var currentUserId = _currentUserService.UserId!.Value;

        var exists = await _projectRepository.GetByNameAndCreatorAsync(request.Name, currentUserId);
        if (exists != null)
            throw new InvalidOperationException("You already have a project with the same name");

        var project = new Project
        {
            ProjectId = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            CategoryId = request.CategoryId,
            TemplateId = request.TemplateId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = currentUserId
        };

        await _projectRepository.CreateAsync(project);
        await _projectRepository.SaveChangesAsync();

        await _eventDispatcher.DispatchAsync(new ProjectCreatedEvent(project.ProjectId));

        project = await _projectRepository.GetByIdAsync(project.ProjectId);

        return MapToResponse(project!);
    }

    public async Task<ProjectResponse?> UpdateProject(Guid id, UpdateProjectRequest request)
    {
        var project = await _projectRepository.GetByIdAsync(id);
        if (project == null) return null;

        var currentUserId = _currentUserService.UserId!.Value;

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            var exists = _projectRepository.GetByNameAndCreatorAsync(request.Name, currentUserId);
            if (exists != null)
                throw new InvalidOperationException("You already have a project with the same name");
            project.Name = request.Name;
        }

        if (!string.IsNullOrWhiteSpace(request.Description))
        {
            project.Description = request.Description;
        }

        if (request.IsActive.HasValue)
        {
            project.IsActive = request.IsActive.Value;
        }

        await _projectRepository.UpdateAsync(project);
        await _projectRepository.SaveChangesAsync();

        return MapToResponse(project);
    }

    public async Task DeleteProject(Guid id)
    {
        var project = await _projectRepository.GetByIdAsync(id);
        if (project == null)
            throw new KeyNotFoundException("Project not found");

        await _projectRepository.DeleteAsync(project);
        await _projectRepository.SaveChangesAsync();

        await _eventDispatcher.DispatchAsync(new ProjectDeletedEvent(id));
    }

    private static ProjectResponse MapToResponse(Project p) =>
        new ProjectResponse
        {
            ProjectId = p.ProjectId,
            Name = p.Name,
            Description = p.Description,
            IsActive = p.IsActive,
            CreatedAt = p.CreatedAt,
            CreatedBy = p.CreatedBy,
            Category = new()
            {
                CategoryId = p.CategoryId,
                Name = p.ProjectCategory.Name,
                Description = p.ProjectCategory.Description,
                CreatedAt = p.ProjectCategory.CreatedAt,
                IsActive = p.ProjectCategory.IsActive
            },
            Template = new()
            {
                TemplateId = p.TemplateId,
                Name = p.ProjectTemplate.Name,
                MediaType = p.ProjectTemplate.MediaType.ToString()
            }
        };
}