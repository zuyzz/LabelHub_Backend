using DataLabelProject.Application.DTOs.Labels;
using DataLabelProject.Business.Models;
using DataLabelProject.Data.Repositories.Abstractions;

namespace DataLabelProject.Business.Services.Labels;

public class LabelSetService : ILabelSetService
{
    private readonly ILabelSetRepository _labelSetRepository;

    public LabelSetService(ILabelSetRepository labelSetRepository)
    {
        _labelSetRepository = labelSetRepository;
    }

    public async Task<List<LabelSetResponse>> GetAllAsync()
    {
        var list = await _labelSetRepository.GetAllAsync();

        return list.Select(ls => new LabelSetResponse
        {
            LabelSetId = ls.LabelSetId,
            Name = ls.Name,
            Description = ls.Description,
            ProjectId = ls.ProjectId,
            CreatedAt = ls.CreatedAt
        }).ToList();
    }

    public async Task<LabelSetResponse> CreateAsync(Guid projectId, CreateLabelSetRequest request, Guid? createdBy)
    {
        var labelSet = new LabelSet
        {
            LabelSetId = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            ProjectId = request.ProjectId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        await _labelSetRepository.CreateAsync(labelSet);
        await _labelSetRepository.SaveChangesAsync();

        return new LabelSetResponse
        {
            LabelSetId = labelSet.LabelSetId,
            Name = labelSet.Name,
            Description = labelSet.Description,
            ProjectId = labelSet.ProjectId,
            CreatedAt = labelSet.CreatedAt
        };
    }

}
