using DataLabel_Project_BE.Models;
using DataLabel_Project_BE.Repositories;

namespace DataLabel_Project_BE.Services
{
    public class LabelService : ILabelService
    {
        private readonly ILabelRepository _labelRepository;

        public LabelService(ILabelRepository labelRepository)
        {
            _labelRepository = labelRepository;
        }

        public async Task<List<Label>> GetLabelsByLabelSetAsync(Guid labelSetId)
        {
            return await _labelRepository.GetByLabelSetIdAsync(labelSetId);
        }

        public async Task<Label> CreateLabelAsync(Guid labelSetId, string name)
        {
            var label = new Label
            {
                LabelId = Guid.NewGuid(),
                LabelSetId = labelSetId,
                Name = name,
                IsActive = true
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

