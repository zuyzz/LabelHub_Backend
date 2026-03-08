using DataLabelProject.Business.Models;
using DataLabelProject.Data.Repositories.Abstractions;

namespace DataLabelProject.Business.Services.Labels
{
    public class LabelService : ILabelService
    {
        private readonly ILabelRepository _labelRepository;

        public LabelService(ILabelRepository labelRepository)
        {
            _labelRepository = labelRepository;
        }

        public async Task<List<Label>> GetAllLabelsAsync()
        {
            return await _labelRepository.GetAllAsync();
        }

        public async Task<List<Label>> GetLabelsByCategoryAsync(Guid categoryId)
        {
            return await _labelRepository.GetByCategoryIdAsync(categoryId);
        }

        public async Task<Label> CreateLabelAsync(Guid categoryId, string name, Guid createdBy)
        {
            var label = new Label
            {
                LabelId = Guid.NewGuid(),
                CategoryId = categoryId,
                Name = name,
                IsActive = true,
                CreatedBy = createdBy
            };

            await _labelRepository.AddAsync(label);
            return label;
        }

        public async Task UpdateLabelAsync(Guid labelId, string name, bool isActive)
        {
            var label = await _labelRepository.GetByIdAsync(labelId)
                ?? throw new Exception("Label not found");

            label.Name = name;
            label.IsActive = isActive;

            await _labelRepository.UpdateAsync(label);
        }

        public async Task DeleteLabelAsync(Guid labelId)
        {
            var label = await _labelRepository.GetByIdAsync(labelId)
                ?? throw new Exception("Label not found");

            label.IsActive = false;
            await _labelRepository.UpdateAsync(label);
        }
    }
}
