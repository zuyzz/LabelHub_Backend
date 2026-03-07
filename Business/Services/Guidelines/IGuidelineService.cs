using DataLabelProject.Application.DTOs.Guidelines;

namespace DataLabelProject.Business.Services.Guidelines;

public interface IGuidelineService
{
    Task<List<GuidelineResponse>> GetAllGuidelines();
    Task<GuidelineResponse?> GetGuidelineById(Guid id);
    
    /// <summary>
    /// Retrieve the guideline associated with a specific project (if exists)
    /// </summary>
    Task<GuidelineResponse?> GetGuidelineByProjectAsync(Guid projectId);

    Task<GuidelineResponse> CreateGuideline(CreateGuidelineRequest request);
    Task<GuidelineResponse?> UpdateGuideline(Guid id, UpdateGuidelineRequest request);
    Task<(bool Success, string Message)> DeleteGuideline(Guid id);
}
