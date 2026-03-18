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

        public async Task<ProjectConfig?> GetByProjectIdAsync(Guid projectId)
        {
            return await _context.ProjectConfigs
                .FirstOrDefaultAsync(c => c.ProjectId == projectId);
        }

        public async Task CreateAsync(ProjectConfig config)
        {
            await _context.AddAsync(config);
        }

        public async Task DeleteAsync(ProjectConfig config)
        {
            _context.Remove(config);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}