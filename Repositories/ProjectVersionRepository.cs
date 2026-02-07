using DataLabel_Project_BE.Data;
using DataLabel_Project_BE.Models;
using Microsoft.EntityFrameworkCore;

namespace DataLabel_Project_BE.Repositories
{
    public class ProjectVersionRepository : IProjectVersionRepository
    {
        private readonly AppDbContext _context;

        public ProjectVersionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ProjectVersion?> GetByIdAsync(Guid projectVersionId)
        {
            return await _context.Set<ProjectVersion>()
                .Include(pv => pv.Dataset)
                .Include(pv => pv.LabelSet)
                .Include(pv => pv.Guideline)
                .FirstOrDefaultAsync(pv => pv.ProjectVersionId == projectVersionId);
        }

        public async Task<ProjectVersion?> GetDraftByProjectIdAsync(Guid projectId)
        {
            return await _context.Set<ProjectVersion>()
                .Where(pv => pv.ProjectId == projectId && pv.ReleasedAt == null)
                .OrderByDescending(pv => pv.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<ProjectVersion?> GetLatestReleasedByProjectIdAsync(Guid projectId)
        {
            return await _context.Set<ProjectVersion>()
                .Where(pv => pv.ProjectId == projectId && pv.ReleasedAt != null)
                .OrderByDescending(pv => pv.VersionNumber)
                .FirstOrDefaultAsync();
        }

        public async Task<int> GetNextVersionNumberAsync(Guid projectId)
        {
            var maxVersion = await _context.Set<ProjectVersion>()
                .Where(pv => pv.ProjectId == projectId)
                .MaxAsync(pv => (int?)pv.VersionNumber);

            return (maxVersion ?? 0) + 1;
        }

        public async Task<IEnumerable<ProjectVersion>> GetAllByProjectIdAsync(Guid projectId)
        {
            return await _context.Set<ProjectVersion>()
                .Where(pv => pv.ProjectId == projectId)
                .OrderByDescending(pv => pv.VersionNumber)
                .ToListAsync();
        }

        public async Task AddAsync(ProjectVersion projectVersion)
        {
            await _context.Set<ProjectVersion>().AddAsync(projectVersion);
        }

        public Task UpdateAsync(ProjectVersion projectVersion)
        {
            _context.Set<ProjectVersion>().Update(projectVersion);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
