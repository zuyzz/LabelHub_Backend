using DataLabel_Project_BE.Data;
using DataLabel_Project_BE.Models;
using Microsoft.EntityFrameworkCore;

namespace DataLabel_Project_BE.Repositories
{
    public class LabelRepository : ILabelRepository
    {
        private readonly AppDbContext _context;

        public LabelRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Label>> GetByLabelSetIdAsync(Guid labelSetId)
        {
            return await _context.Labels
                .Where(l => l.LabelSetId == labelSetId)
                .ToListAsync();
        }

        public async Task<Label?> GetByIdAsync(Guid labelId)
        {
            return await _context.Labels
                .FirstOrDefaultAsync(l => l.LabelId == labelId);
        }

        public async Task AddAsync(Label label)
        {
            _context.Labels.Add(label);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Label label)
        {
            _context.Labels.Update(label);
            await _context.SaveChangesAsync();
        }
    }
}
