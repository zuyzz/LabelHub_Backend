using DataLabelProject.Application.DTOs.Labels;

namespace DataLabelProject.Business.Services.Labels;

public interface ILabelSetService
{
    Task<List<LabelSetResponse>> GetAllAsync();
    Task<LabelSetResponse> CreateAsync(Guid projectId, CreateLabelSetRequest request, Guid? createdBy);
}
