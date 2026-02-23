using DataLabel_Project_BE.DTOs;

namespace DataLabel_Project_BE.Services;

public interface ILabelSetService
{
    Task<List<LabelSetResponse>> GetAllAsync();
    Task<LabelSetResponse> CreateAsync(Guid projectId, CreateLabelSetRequest request, Guid? createdBy);
}
