using DataLabelProject.Application.DTOs.Projects;
using DataLabelProject.Application.DTOs.Categories;
using DataLabelProject.Application.DTOs.Common;
using DataLabelProject.Business.Models;
using DataLabelProject.Data.Repositories.Abstractions;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace DataLabelProject.Business.Services.Projects
{
    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository _projectRepo;
        private readonly ICategoryRepository _categoryRepo;
        private readonly IProjectTemplateRepository _templateRepo;
        private readonly IHttpContextAccessor _httpContextAccessor;

        // Valid project status values
        private static readonly HashSet<string> ValidStatuses = new HashSet<string> { "opened", "archived" };

        public ProjectService(
            IProjectRepository projectRepo,
            ICategoryRepository categoryRepo,
            IProjectTemplateRepository templateRepo,
            IHttpContextAccessor httpContextAccessor)
        {
            _projectRepo = projectRepo;
            _categoryRepo = categoryRepo;
            _templateRepo = templateRepo;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<ProjectResponse>> GetAllAsync()
        {
            var projects = await _projectRepo.GetAllAsync();
            return projects.Select(MapToResponse);
        }

        public async Task<PagedResponse<ProjectResponse>> GetProjectsAsync(ProjectQueryParameters query)
        {
            if (query.Page < 1) query.Page = 1;

            // Check if current user is admin
            var user = _httpContextAccessor.HttpContext?.User;
            var isAdmin = user?.IsInRole("admin") == true;

            var (items, total) = isAdmin
                ? await _projectRepo.GetFilteredAsync(query)  // Show all projects for admin
                : await _projectRepo.GetUserProjectsAsync(query, GetCurrentUserId());  // Show only user's projects for non-admin

            var mapped = items.Select(MapToResponse);

            return new PagedResponse<ProjectResponse>
            {
                Items = mapped,
                TotalItems = total,
                Page = query.Page,
                PageSize = query.PageSize,
                TotalPages = (int)Math.Ceiling((double)total / query.PageSize)
            };
        }

        public async Task<ProjectResponse?> GetByIdAsync(Guid id)
        {
            var p = await _projectRepo.GetByIdAsync(id);
            return p is null ? null : MapToResponse(p);
        }

        public async Task<ProjectResponse> CreateAsync(ProjectCreateRequest dto)
        {
            // Validate Category exists
            var category = await _categoryRepo.GetByIdAsync(dto.CategoryId);
            if (category == null)
                throw new InvalidOperationException($"Category {dto.CategoryId} not found");

            // Validate Template exists
            var template = await _templateRepo.GetByIdAsync(dto.TemplateId);
            if (template == null)
                throw new InvalidOperationException($"ProjectTemplate {dto.TemplateId} not found");

            Guid? createdBy = null;
            var userIdClaim = _httpContextAccessor.HttpContext?
                .User?
                .FindFirst(ClaimTypes.NameIdentifier)?
                .Value;

            if (Guid.TryParse(userIdClaim, out var parsed))
                createdBy = parsed;

            var project = new Project
            {
                ProjectId = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description,
                Status = "opened",
                CategoryId = dto.CategoryId,
                TemplateId = dto.TemplateId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            };

            await _projectRepo.AddAsync(project);
            await _projectRepo.SaveChangesAsync();

            // Add creator as project member (non-admin only)
            if (createdBy.HasValue &&
                _httpContextAccessor.HttpContext?.User?.IsInRole("admin") != true)
            {
                var added = await _projectRepo.AddProjectMemberAsync(
                    project.ProjectId,
                    createdBy.Value
                );

                if (added)
                    await _projectRepo.SaveChangesAsync();
            }

            // Reload to get full objects
            project = await _projectRepo.GetByIdAsync(project.ProjectId);
            return MapToResponse(project!);
        }

        public async Task<PagedResponse<ProjectResponse>> GetUserProjectsAsync(ProjectQueryParameters query)
        {
            if (query.Page < 1) query.Page = 1;

            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return new PagedResponse<ProjectResponse>
                {
                    Items = Array.Empty<ProjectResponse>(),
                    TotalItems = 0,
                    Page = query.Page,
                    PageSize = query.PageSize,
                    TotalPages = 0
                };
            }

            var (items, total) = await _projectRepo.GetUserProjectsAsync(query, userId);
            var mapped = items.Select(MapToResponse);

            return new PagedResponse<ProjectResponse>
            {
                Items = mapped,
                TotalItems = total,
                Page = query.Page,
                PageSize = query.PageSize,
                TotalPages = (int)Math.Ceiling((double)total / query.PageSize)
            };
        }

        public async Task<JoinProjectResult> JoinProjectAsync(Guid projectId)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var userIdClaim = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId)) return JoinProjectResult.Unauthorized;

            // Admins are not allowed to join projects
            if (user?.IsInRole("admin") == true) return JoinProjectResult.Forbidden;

            if (!await _projectRepo.ExistsAsync(projectId)) return JoinProjectResult.ProjectNotFound;

            var added = await _projectRepo.AddProjectMemberAsync(projectId, userId);
            if (!added) return JoinProjectResult.AlreadyMember;

            await _projectRepo.SaveChangesAsync();
            return JoinProjectResult.Success;
        }

        public async Task<ProjectResponse?> UpdateAsync(Guid id, ProjectUpdateRequest dto)
        {
            var project = await _projectRepo.GetByIdAsync(id);
            if (project == null) return null;

            // Validate Status if provided
            if (!string.IsNullOrWhiteSpace(dto.Status) && !ValidStatuses.Contains(dto.Status))
                throw new InvalidOperationException($"Invalid project status: {dto.Status}. Valid values: {string.Join(", ", ValidStatuses)}");

            project.Name = dto.Name;
            project.Description = dto.Description;

            if (!string.IsNullOrWhiteSpace(dto.Status))
                project.Status = dto.Status!;

            await _projectRepo.UpdateAsync(project);
            await _projectRepo.SaveChangesAsync();

            // Reload to get full objects
            project = await _projectRepo.GetByIdAsync(id);
            return MapToResponse(project!);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var project = await _projectRepo.GetByIdAsync(id);
            if (project == null) return false;

            await _projectRepo.DeleteAsync(project);
            await _projectRepo.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<ProjectMemberResponse>> GetProjectMembersAsync(Guid projectId)
        {
            var members = await _projectRepo.GetActiveProjectMembersAsync(projectId);
            return members.Select(MapMemberToResponse);
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }

        private static ProjectResponse MapToResponse(Project p) =>
            new ProjectResponse
            {
                ProjectId = p.ProjectId,
                Name = p.Name,
                Description = p.Description,
                Status = p.Status,
                CreatedAt = p.CreatedAt,
                CreatedBy = p.CreatedBy,
                Category = MapCategoryToResponse(p.ProjectCategory),
                Template = MapTemplateToResponse(p.ProjectTemplate)
            };

        private static CategoryResponse MapCategoryToResponse(Category c) =>
            new CategoryResponse
            {
                CategoryId = c.CategoryId,
                Name = c.Name,
                Description = c.Description,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt
            };

        private static ProjectTemplateResponse MapTemplateToResponse(ProjectTemplate t) =>
            new ProjectTemplateResponse
            {
                TemplateId = t.TemplateId,
                Name = t.Name,
                MediaType = t.MediaType.ToString()
            };

        private static ProjectMemberResponse MapMemberToResponse(ProjectMember pm) =>
            new ProjectMemberResponse
            {
                ProjectMemberId = pm.ProjectMemberId,
                UserId = pm.ProjectMemberUser.UserId,
                Username = pm.ProjectMemberUser.Username,
                DisplayName = pm.ProjectMemberUser.DisplayName,
                Email = pm.ProjectMemberUser.Email,
                PhoneNumber = pm.ProjectMemberUser.PhoneNumber,
                RoleId = pm.ProjectMemberUser.RoleId,
                RoleName = pm.ProjectMemberUser.UserRole?.RoleName ?? string.Empty,
                IsActive = pm.ProjectMemberUser.IsActive,
                JoinedAt = pm.JoinedAt
            };
    }
}
