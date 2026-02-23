using DataLabelProject.Application.DTOs.Guidelines;

namespace DataLabelProject.Business.Services.Guidelines;

public interface IGuidelineService
{
    Task<List<GuidelineResponse>> GetAllGuidelines();
    Task<GuidelineResponse?> GetGuidelineById(Guid id);
    Task<GuidelineResponse> CreateGuideline(CreateGuidelineRequest request);
    Task<GuidelineResponse?> UpdateGuideline(Guid id, UpdateGuidelineRequest request);
    Task<(bool Success, string Message)> DeleteGuideline(Guid id);
}
