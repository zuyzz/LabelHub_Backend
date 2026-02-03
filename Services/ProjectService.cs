using DataLabel_Project_BE.DTOs;
using DataLabel_Project_BE.Models;
using DataLabel_Project_BE.Repositories;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace DataLabel_Project_BE.Services
{
    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository _repo;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProjectService(IProjectRepository repo, IHttpContextAccessor httpContextAccessor)
        {
            _repo = repo;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<ProjectResponse>> GetAllAsync()
        {
            var projects = await _repo.GetAllAsync();
            return projects.Select(MapToResponse);
        }

        public async Task<PagedResponse<ProjectResponse>> GetProjectsAsync(ProjectQueryParameters query)
        {
            if (query.Page < 1) query.Page = 1;

            var (items, total) = await _repo.GetFilteredAsync(query);

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
            var p = await _repo.GetByIdAsync(id);
            return p is null ? null : MapToResponse(p);
        }

        public async Task<ProjectResponse> CreateAsync(ProjectCreateRequest dto)
        {
            Guid? createdBy = null;
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdClaim, out var parsed)) createdBy = parsed;

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

            await _repo.AddAsync(project);
            await _repo.SaveChangesAsync();

            // If project created by a user (manager/admin), add them to ProjectMember table
            if (createdBy.HasValue)
            {
                var added = await _repo.AddProjectMemberAsync(project.ProjectId, createdBy.Value);
                if (added)
                {
                    await _repo.SaveChangesAsync();
                }
            }

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

            var (items, total) = await _repo.GetUserProjectsAsync(query, userId);
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
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId)) return DTOs.JoinProjectResult.Unauthorized;

            if (!await _repo.ExistsAsync(projectId)) return DTOs.JoinProjectResult.ProjectNotFound;

            var added = await _repo.AddProjectMemberAsync(projectId, userId);
            if (!added) return DTOs.JoinProjectResult.AlreadyMember;

            await _repo.SaveChangesAsync();
            return DTOs.JoinProjectResult.Success;
        }

        public async Task<ProjectResponse?> UpdateAsync(Guid id, ProjectUpdateRequest dto)
        {
            var project = await _repo.GetByIdAsync(id);
            if (project == null) return null;

            project.Name = dto.Name;
            project.Description = dto.Description;
            if (!string.IsNullOrWhiteSpace(dto.Status)) project.Status = dto.Status!;
            project.CategoryId = dto.CategoryId;

            await _repo.UpdateAsync(project);
            await _repo.SaveChangesAsync();

            return MapToResponse(project);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var project = await _repo.GetByIdAsync(id);
            if (project == null) return false;

            await _repo.DeleteAsync(project);
            await _repo.SaveChangesAsync();
            return true;
        }

        private static ProjectResponse MapToResponse(Project p) =>
            new ProjectResponse(p.ProjectId, p.Name, p.Description, p.Status, p.CategoryId, p.CreatedAt, p.CreatedBy);
    }
} 