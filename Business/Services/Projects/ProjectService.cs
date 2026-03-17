using DataLabelProject.Application.DTOs.Projects;
using DataLabelProject.Application.DTOs.Common;
using DataLabelProject.Business.Models;
using DataLabelProject.Data.Repositories.Abstractions;
using DataLabelProject.Business.Services.Users;

namespace DataLabelProject.Business.Services.Projects;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectMemberRepository _projectMemberRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IProjectTemplateRepository _templateRepository;
    private readonly ICurrentUserService _currentUserService;

    public ProjectService (
        IProjectRepository projectRepository,
        IProjectMemberRepository projectMemberRepository,
        ICategoryRepository categoryRepository,
        IProjectTemplateRepository templateRepository,
        ICurrentUserService currentUserService)
    {
        _projectRepository = projectRepository;
        _projectMemberRepository = projectMemberRepository;
        _categoryRepository = categoryRepository;
        _templateRepository = templateRepository;
        _currentUserService = currentUserService;
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
            TotalPages = (int)Math.Ceiling((double)totalCount / @params.PageSize)
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

        var member = new ProjectMember
        {
            ProjectId = project.ProjectId,
            MemberId = currentUserId,
            JoinedAt = DateTime.UtcNow
        };

        // add manager as a member
        await _projectMemberRepository.CreateAsync(member);
        await _projectMemberRepository.SaveChangesAsync();

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

    public async Task<bool> DeleteProject(Guid id)
    {
        var project = await _projectRepository.GetByIdAsync(id);
        if (project == null) return false;

        await _projectRepository.DeleteAsync(project);
        await _projectRepository.SaveChangesAsync();

        return true;
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