using DataLabel_Project_BE.DTOs.Guideline;
using DataLabel_Project_BE.Models;
using DataLabel_Project_BE.Repositories;
using DataLabel_Project_BE.Services;

namespace DataLabel_Project_BE.Services;

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
            Version = g.Version,
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
            Version = guideline.Version,
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
            Version = 1,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(guideline);

        return new GuidelineResponse
        {
            GuidelineId = guideline.GuidelineId,
            Title = guideline.Title,
            Content = guideline.Content,
            Version = guideline.Version,
            CreatedAt = guideline.CreatedAt
        };
    }

    public async Task<GuidelineResponse?> UpdateGuideline(Guid id, UpdateGuidelineRequest request)
    {
        var guideline = await _repository.GetByIdAsync(id);
        if (guideline == null) return null;

        guideline.Title = request.Title;
        guideline.Content = request.Content;
        guideline.Version++;

        await _repository.UpdateAsync(guideline);

        return new GuidelineResponse
        {
            GuidelineId = guideline.GuidelineId,
            Title = guideline.Title,
            Content = guideline.Content,
            Version = guideline.Version,
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
