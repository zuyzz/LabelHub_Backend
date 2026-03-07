using DataLabelProject.Business.Models;
using DataLabelProject.Data.Repositories.Abstractions;

namespace DataLabelProject.Business.Services.Labels
{
    public class LabelService : ILabelService
    {
        private readonly ILabelRepository _labelRepository;
        private readonly ILabelSetRepository _labelSetRepository;

        public LabelService(ILabelRepository labelRepository, ILabelSetRepository labelSetRepository)
        {
            _labelRepository = labelRepository;
            _labelSetRepository = labelSetRepository;
        }

        public async Task<List<Label>> GetLabelsByLabelSetAsync(Guid labelSetId)
        {
            return await _labelRepository.GetByLabelSetIdAsync(labelSetId);
        }

        public async Task<Label> CreateLabelAsync(Guid labelSetId, string name)
        {
            // Check if name already exists in the label set
            var existingLabel = await _labelRepository.GetByNameAndLabelSetAsync(labelSetId, name);
            if (existingLabel != null)
            {
                throw new Exception("Label name must be unique within the label set");
            }

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

            // Check if name is changing and if the new name already exists in the label set
            if (label.Name != name)
            {
                var existingLabel = await _labelRepository.GetByNameAndLabelSetAsync(label.LabelSetId, name);
                if (existingLabel != null)
                {
                    throw new Exception("Label name must be unique within the label set");
                }
            }

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

        public async Task<LabelSet?> GetLabelSetByIdAsync(Guid labelSetId)
        {
            return await _labelSetRepository.GetByIdAsync(labelSetId);
        }
    }
}
