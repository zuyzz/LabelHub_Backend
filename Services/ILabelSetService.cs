using DataLabel_Project_BE.DTOs;

namespace DataLabel_Project_BE.Services;

public interface ILabelSetService
{
    Task<List<LabelSetResponse>> GetAllAsync();
    Task<LabelSetResponse> CreateAsync(CreateLabelSetRequest request, Guid? createdBy);
    Task<LabelSetResponse?> CreateNewVersionAsync(Guid labelSetId, UpdateLabelSetRequest request, Guid? createdBy);
}
