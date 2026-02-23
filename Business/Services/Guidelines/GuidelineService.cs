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

    public async Task<List<GuidelineResponse>> GetAllGuidelines()
    {
        var guidelines = await _repository.GetAllAsync();

        return guidelines.Select(g => new GuidelineResponse
        {
            GuidelineId = g.GuidelineId,
            Title = g.Title,
            Content = g.Content,
            CreatedAt = g.CreatedAt
        }).ToList();
    }

    public async Task<GuidelineResponse?> GetGuidelineById(Guid id)
    {
        var guideline = await _repository.GetByIdAsync(id);
        if (guideline == null) return null;

        return new GuidelineResponse
        {
            GuidelineId = guideline.GuidelineId,
            Title = guideline.Title,
            Content = guideline.Content,
            CreatedAt = guideline.CreatedAt
        };
    }

    public async Task<GuidelineResponse> CreateGuideline(CreateGuidelineRequest request)
    {
        var guideline = new Guideline
        {
            GuidelineId = Guid.NewGuid(),
            Title = request.Title,
            Content = request.Content,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(guideline);

        return new GuidelineResponse
        {
            GuidelineId = guideline.GuidelineId,
            Title = guideline.Title,
            Content = guideline.Content,
            CreatedAt = guideline.CreatedAt
        };
    }

    public async Task<GuidelineResponse?> UpdateGuideline(Guid id, UpdateGuidelineRequest request)
    {
        var guideline = await _repository.GetByIdAsync(id);
        if (guideline == null) return null;

        guideline.Title = request.Title;
        guideline.Content = request.Content;

        await _repository.UpdateAsync(guideline);

        return new GuidelineResponse
        {
            GuidelineId = guideline.GuidelineId,
            Title = guideline.Title,
            Content = guideline.Content,
            CreatedAt = guideline.CreatedAt
        };
    }

    public async Task<(bool Success, string Message)> DeleteGuideline(Guid id)
    {
        var guideline = await _repository.GetByIdAsync(id);
        if (guideline == null)
            return (false, "Guideline not found");

        if (await _repository.IsGuidelineInUseAsync(id))
            return (false, "Cannot delete guideline that is being used by label sets");

        await _repository.DeleteAsync(guideline);
        return (true, "Guideline deleted successfully");
    }
}
