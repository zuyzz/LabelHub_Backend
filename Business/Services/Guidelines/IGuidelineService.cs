using DataLabelProject.Application.DTOs.Common;
using DataLabelProject.Application.DTOs.Guidelines;

namespace DataLabelProject.Business.Services.Guidelines;

public interface IGuidelineService
{
    Task<PagedResponse<GuidelineResponse>> GetGuidelines(GuidelineQueryParameters @params);
    Task<GuidelineResponse?> GetProjectGuideline(Guid projectId);
    Task<GuidelineResponse?> GetGuidelineById(Guid id);
    Task<GuidelineResponse> CreateGuideline(CreateGuidelineRequest request);
    Task<GuidelineResponse?> UpdateGuideline(Guid id, UpdateGuidelineRequest request);
    Task<bool> DeleteGuideline(Guid id);
}
