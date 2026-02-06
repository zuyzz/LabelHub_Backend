using DataLabel_Project_BE.DTOs;
using DataLabel_Project_BE.Models;
using DataLabel_Project_BE.Repositories;

namespace DataLabel_Project_BE.Services;

public class LabelSetService : ILabelSetService
{
    private readonly ILabelSetRepository _repository;

    public LabelSetService(ILabelSetRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<LabelSetResponse>> GetAllAsync()
    {
        var list = await _repository.GetAllAsync();

        return list.Select(ls => new LabelSetResponse
        {
            LabelSetId = ls.LabelSetId,
            Name = ls.Name,
            Description = ls.Description,
            VersionNumber = ls.VersionNumber,
            GuidelineId = ls.GuidelineId,
            CreatedAt = ls.CreatedAt
        }).ToList();
    }

    public async Task<LabelSetResponse> CreateAsync(CreateLabelSetRequest request, Guid? createdBy)
    {
        var labelSet = new Models.LabelSet
        {
            LabelSetId = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            GuidelineId = request.GuidelineId,
            VersionNumber = 1,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        await _repository.CreateAsync(labelSet);

        return new LabelSetResponse
        {
            LabelSetId = labelSet.LabelSetId,
            Name = labelSet.Name,
            Description = labelSet.Description,
            VersionNumber = labelSet.VersionNumber,
            GuidelineId = labelSet.GuidelineId,
            CreatedAt = labelSet.CreatedAt
        };
    }

    public async Task<LabelSetResponse?> CreateNewVersionAsync(
        Guid labelSetId,
        UpdateLabelSetRequest request,
        Guid? createdBy)
    {
        var latest = await _repository.GetLatestVersionAsync(labelSetId);
        if (latest == null) return null;

        var newVersion = new Models.LabelSet
        {
            LabelSetId = latest.LabelSetId, // GIỮ NGUYÊN
            Name = request.Name,
            Description = request.Description,
            GuidelineId = request.GuidelineId,
            VersionNumber = latest.VersionNumber + 1,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        await _repository.CreateAsync(newVersion);

        return new LabelSetResponse
        {
            LabelSetId = newVersion.LabelSetId,
            Name = newVersion.Name,
            Description = newVersion.Description,
            VersionNumber = newVersion.VersionNumber,
            GuidelineId = newVersion.GuidelineId,
            CreatedAt = newVersion.CreatedAt
        };
    }
}
