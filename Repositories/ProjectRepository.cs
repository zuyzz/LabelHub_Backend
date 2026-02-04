using DataLabel_Project_BE.Data;
using DataLabel_Project_BE.Models;
using Microsoft.EntityFrameworkCore;

namespace DataLabel_Project_BE.Repositories
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly AppDbContext _context;

        public ProjectRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Project>> GetAllAsync()
        {
            return await _context.Projects.ToListAsync();
        }

        public async Task<(IEnumerable<Project> Items, int TotalCount)> GetFilteredAsync(DTOs.ProjectQueryParameters query)
        {
            var q = _context.Projects.AsQueryable();

            // Search by name (case-insensitive)
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var s = query.Search.Trim();
                q = q.Where(p => EF.Functions.ILike(p.Name, $"%{s}%"));
            }

            // Filter by single category id or multiple
            if (query.CategoryId.HasValue)
            {
                q = q.Where(p => p.CategoryId == query.CategoryId.Value);
            }

            if (query.CategoryIds != null && query.CategoryIds.Any())
            {
                q = q.Where(p => query.CategoryIds.Contains(p.CategoryId));
            }

            // Filter by status
            if (!string.IsNullOrWhiteSpace(query.Status))
            {
                q = q.Where(p => p.Status == query.Status);
            }

            // TODO: support additional filters (createdBy, date ranges...) if needed

            // Get total count before paging
            var total = await q.CountAsync();

            // Default sort: newest first (createdAt desc)
            q = q.OrderByDescending(p => p.CreatedAt);

            var skip = (Math.Max(query.Page, 1) - 1) * query.PageSize;
            var items = await q.Skip(skip).Take(query.PageSize).ToListAsync();

            return (items, total);
        }

        public async Task<(IEnumerable<Project> Items, int TotalCount)> GetUserProjectsAsync(DTOs.ProjectQueryParameters query, Guid userId)
        {
            // Join projects with project members to restrict to projects joined by user
            var q = from p in _context.Projects
                    join pm in _context.ProjectMembers on p.ProjectId equals pm.ProjectId
                    where pm.UserId == userId
                    select p;

            // Search by name (case-insensitive)
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var s = query.Search.Trim();
                q = q.Where(p => EF.Functions.ILike(p.Name, $"%{s}%"));
            }

            // Filter by single category id or multiple
            if (query.CategoryId.HasValue)
            {
                q = q.Where(p => p.CategoryId == query.CategoryId.Value);
            }

            if (query.CategoryIds != null && query.CategoryIds.Any())
            {
                q = q.Where(p => query.CategoryIds.Contains(p.CategoryId));
            }

            // Filter by status
            if (!string.IsNullOrWhiteSpace(query.Status))
            {
                q = q.Where(p => p.Status == query.Status);
            }

            // Count then page
            var total = await q.CountAsync();
            q = q.OrderByDescending(p => p.CreatedAt);
            var skip = (Math.Max(query.Page, 1) - 1) * query.PageSize;
            var items = await q.Skip(skip).Take(query.PageSize).ToListAsync();

            return (items, total);
        }

        public async Task<bool> ProjectMemberExistsAsync(Guid projectId, Guid userId)
        {
            return await _context.ProjectMembers.AnyAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);
        }

        public async Task<bool> AddProjectMemberAsync(Guid projectId, Guid userId)
        {
            if (await ProjectMemberExistsAsync(projectId, userId)) return false;

            var pm = new ProjectMember
            {
                ProjectMemberId = Guid.NewGuid(),
                ProjectId = projectId,
                UserId = userId,
                JoinedAt = DateTime.UtcNow
            };

            await _context.ProjectMembers.AddAsync(pm);
            return true;
        }

        public async Task<Project?> GetByIdAsync(Guid id)
        {
            return await _context.Projects.FindAsync(id);
        }

        public async Task AddAsync(Project project)
        {
            await _context.Projects.AddAsync(project);
        }

        public Task UpdateAsync(Project project)
        {
            _context.Projects.Update(project);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Project project)
        {
            _context.Projects.Remove(project);
            return Task.CompletedTask;
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Projects.AnyAsync(p => p.ProjectId == id);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}