using DataLabelProject.Application.DTOs.Projects;

namespace DataLabelProject.Business.Services.Projects
{
    public interface IProjectTemplateService
    {
        Task<IEnumerable<ProjectTemplateResponse>> GetAllAsync();
        Task<ProjectTemplateResponse?> GetByIdAsync(Guid id);
        Task<ProjectTemplateResponse> CreateAsync(ProjectTemplateCreateRequest dto);
        Task<ProjectTemplateResponse?> UpdateAsync(Guid id, ProjectTemplateUpdateRequest dto);
        Task<bool> DeleteAsync(Guid id);
    }
}
