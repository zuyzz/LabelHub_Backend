using DataLabelProject.Business.Models;
using DataLabelProject.Data.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace DataLabelProject.Data.Repositories.Implementations.ProjectConfigs
{
    public class ProjectConfigRepository : IProjectConfigRepository
    {
        private readonly AppDbContext _context;

        public ProjectConfigRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ProjectConfig?> GetLatestByProjectIdAsync(Guid projectId)
        {
            return await _context.ProjectConfigs
                .Where(c => c.ProjectId == projectId)
                .OrderByDescending(c => c.ProjectConfigId)
                .FirstOrDefaultAsync();
        }
    }
}