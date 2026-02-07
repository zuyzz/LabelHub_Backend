using DataLabel_Project_BE.DTOs;
using DataLabel_Project_BE.Models;
using DataLabel_Project_BE.Repositories;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace DataLabel_Project_BE.Services
{
    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository _projectRepo;
        private readonly IProjectVersionService _projectVersionService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProjectService(IProjectRepository projectRepo, IProjectVersionService projectVersionService, IHttpContextAccessor httpContextAccessor)
        {
            _projectRepo = projectRepo;
            _projectVersionService = projectVersionService;
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

            var (items, total) = await _projectRepo.GetFilteredAsync(query);

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
                Status = "active",
                CategoryId = dto.CategoryId,
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

            // create intial draft project version
            await _projectVersionService.CreateDraftAsync(
                projectId: project.ProjectId,
                datasetId: null,
                labelSetId: null,
                guidelineId: null
            );

            return MapToResponse(project);
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

        public async Task<DTOs.JoinProjectResult> JoinProjectAsync(Guid projectId)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var userIdClaim = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId)) return DTOs.JoinProjectResult.Unauthorized;

            // Admins are not allowed to join projects
            if (user?.IsInRole("admin") == true) return DTOs.JoinProjectResult.Forbidden;

            if (!await _projectRepo.ExistsAsync(projectId)) return DTOs.JoinProjectResult.ProjectNotFound;

            var added = await _projectRepo.AddProjectMemberAsync(projectId, userId);
            if (!added) return DTOs.JoinProjectResult.AlreadyMember;

            await _projectRepo.SaveChangesAsync();
            return DTOs.JoinProjectResult.Success;
        }

        public async Task<ProjectResponse?> UpdateAsync(Guid id, ProjectUpdateRequest dto)
        {
            var project = await _projectRepo.GetByIdAsync(id);
            if (project == null) return null;

            project.Name = dto.Name;
            project.Description = dto.Description;
            if (!string.IsNullOrWhiteSpace(dto.Status)) project.Status = dto.Status!;
            project.CategoryId = dto.CategoryId;

            await _projectRepo.UpdateAsync(project);
            await _projectRepo.SaveChangesAsync();

            return MapToResponse(project);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var project = await _projectRepo.GetByIdAsync(id);
            if (project == null) return false;

            await _projectRepo.DeleteAsync(project);
            await _projectRepo.SaveChangesAsync();
            return true;
        }

        private static ProjectResponse MapToResponse(Project p) =>
            new ProjectResponse(p.ProjectId, p.Name, p.Description, p.Status, p.CategoryId, p.CreatedAt, p.CreatedBy);
    }
} 