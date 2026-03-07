using DataLabelProject.Data;
using DataLabelProject.Business.Models;
using DataLabelProject.Data.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace DataLabelProject.Data.Repositories.Implementations.Projects
{
    public class ProjectTemplateRepository : IProjectTemplateRepository
    {
        private readonly AppDbContext _context;

        public ProjectTemplateRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ProjectTemplate>> GetAllAsync()
        {
            return await _context.ProjectTemplates.ToListAsync();
        }

        public async Task<ProjectTemplate?> GetByIdAsync(Guid id)
        {
            return await _context.ProjectTemplates.FirstOrDefaultAsync(t => t.TemplateId == id);
        }

        public async Task<ProjectTemplate> CreateAsync(ProjectTemplate template)
        {
            await _context.ProjectTemplates.AddAsync(template);
            await _context.SaveChangesAsync();
            return template;
        }

        public async Task UpdateAsync(ProjectTemplate template)
        {
            _context.ProjectTemplates.Update(template);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var template = await GetByIdAsync(id);
            if (template != null)
            {
                _context.ProjectTemplates.Remove(template);
                await _context.SaveChangesAsync();
            }
        }
    }
}
