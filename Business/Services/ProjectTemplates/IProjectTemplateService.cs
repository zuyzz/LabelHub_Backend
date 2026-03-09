using DataLabelProject.Application.DTOs.ProjectTemplate;

namespace DataLabelProject.Business.Services.ProjectTemplates;

public interface IProjectTemplateService
{
    Task<IEnumerable<ProjectTemplateResponse>> GetAllAsync();
    Task<ProjectTemplateResponse?> GetByIdAsync(Guid id);
    Task<ProjectTemplateResponse> CreateAsync(CreateProjectTemplateRequest dto);
    Task<ProjectTemplateResponse?> UpdateAsync(Guid id, UpdateProjectTemplateRequest dto);
    Task<bool> DeleteAsync(Guid id);
}
