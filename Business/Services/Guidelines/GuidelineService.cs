using DataLabelProject.Application.DTOs.Common;
using DataLabelProject.Application.DTOs.Guidelines;
using DataLabelProject.Business.Models;
using DataLabelProject.Data.Repositories.Abstractions;

namespace DataLabelProject.Business.Services.Guidelines;

public class GuidelineService : IGuidelineService
{
    private readonly IGuidelineRepository _repository;

    public GuidelineService(IGuidelineRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResponse<GuidelineResponse>> GetGuidelines(GuidelineQueryParameters @params)
    {
        var (items, totalCount) = await _repository.GetAllAsync(@params);

        return new PagedResponse<GuidelineResponse>
        {
            Items = items.Select(MapToResponse).ToList(),
            TotalItems = totalCount,
            Page = @params.Page,
            PageSize = @params.PageSize,
        };
    }

    public async Task<GuidelineResponse?> GetGuidelineById(Guid id)
    {
        var guideline = await _repository.GetByIdAsync(id);
        if (guideline == null) return null;

        return MapToResponse(guideline);
    }

    public async Task<GuidelineResponse> CreateGuideline(CreateGuidelineRequest request)
    {
        var guideline = new Guideline
        {
            GuidelineId = Guid.NewGuid(),
            Content = request.Content,
            CreatedAt = DateTime.UtcNow,
            ProjectId = request.ProjectId
        };

        await _repository.CreateAsync(guideline);
        await _repository.SaveChangesAsync();

        return MapToResponse(guideline);
    }

    public async Task<GuidelineResponse?> UpdateGuideline(Guid id, UpdateGuidelineRequest request)
    {
        var guideline = await _repository.GetByIdAsync(id);
        if (guideline == null) return null;

        if (!string.IsNullOrWhiteSpace(request.Content))
            guideline.Content = request.Content;

        await _repository.UpdateAsync(guideline);
        await _repository.SaveChangesAsync();

        return MapToResponse(guideline);
    }

    public async Task<bool> DeleteGuideline(Guid id)
    {
        var guideline = await _repository.GetByIdAsync(id);
        if (guideline == null) return false;

        await _repository.DeleteAsync(guideline);
        await _repository.SaveChangesAsync();

        return true;
    }

    private GuidelineResponse MapToResponse(Guideline guideline)
    {
        return new GuidelineResponse
        {
            GuidelineId = guideline.GuidelineId,
            Content = guideline.Content,
            CreatedAt = guideline.CreatedAt
        };
    }
}
