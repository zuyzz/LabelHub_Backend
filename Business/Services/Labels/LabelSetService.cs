using DataLabelProject.Application.DTOs.Labels;
using DataLabelProject.Business.Models;
using DataLabelProject.Data.Repositories.Abstractions;

namespace DataLabelProject.Business.Services.Labels;

public class LabelSetService : ILabelSetService
{
    private readonly ILabelSetRepository _labelSetRepository;
    private readonly IProjectVersionRepository _projectVersionRepository;

    public LabelSetService(ILabelSetRepository labelSetRepository, IProjectVersionRepository projectVersionRepository)
    {
        _labelSetRepository = labelSetRepository;
        _projectVersionRepository = projectVersionRepository;
    }

    public async Task<List<LabelSetResponse>> GetAllAsync()
    {
        var list = await _labelSetRepository.GetAllAsync();

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

    public async Task<LabelSetResponse> CreateAsync(Guid projectId, CreateLabelSetRequest request, Guid? createdBy)
    {
        var latest = await _labelSetRepository.GetLatestVersionAsync();
        var verNum = latest?.VersionNumber ?? 0;

        var labelSet = new LabelSet
        {
            LabelSetId = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            GuidelineId = request.GuidelineId,
            VersionNumber = verNum + 1,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        await _labelSetRepository.CreateAsync(labelSet);

        var draftProjectVersion =
            await _projectVersionRepository.GetDraftByProjectIdAsync(projectId);

        if (draftProjectVersion != null)
        {
            draftProjectVersion.LabelSetId = labelSet.LabelSetId;
            await _projectVersionRepository.UpdateAsync(draftProjectVersion);
        }

        await _labelSetRepository.SaveChangesAsync();

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

}
