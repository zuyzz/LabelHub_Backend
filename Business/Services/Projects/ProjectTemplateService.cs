using DataLabelProject.Application.DTOs.Projects;
using DataLabelProject.Business.Models;
using DataLabelProject.Data.Repositories.Abstractions;

namespace DataLabelProject.Business.Services.Projects
{
    public class ProjectTemplateService : IProjectTemplateService
    {
        private readonly IProjectTemplateRepository _repository;

        public ProjectTemplateService(IProjectTemplateRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<ProjectTemplateResponse>> GetAllAsync()
        {
            var templates = await _repository.GetAllAsync();
            return templates.Select(MapToResponse);
        }

        public async Task<ProjectTemplateResponse?> GetByIdAsync(Guid id)
        {
            var template = await _repository.GetByIdAsync(id);
            return template == null ? null : MapToResponse(template);
        }

        public async Task<ProjectTemplateResponse> CreateAsync(ProjectTemplateCreateRequest dto)
        {
            var template = new ProjectTemplate
            {
                TemplateId = Guid.NewGuid(),
                Name = dto.Name,
                MediaType = dto.MediaType
            };

            var created = await _repository.CreateAsync(template);
            return MapToResponse(created);
        }

        public async Task<ProjectTemplateResponse?> UpdateAsync(Guid id, ProjectTemplateUpdateRequest dto)
        {
            var template = await _repository.GetByIdAsync(id);
            if (template == null)
                return null;

            template.Name = dto.Name;
            template.MediaType = dto.MediaType;

            await _repository.UpdateAsync(template);
            return MapToResponse(template);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var template = await _repository.GetByIdAsync(id);
            if (template == null)
                return false;

            await _repository.DeleteAsync(id);
            return true;
        }

        private static ProjectTemplateResponse MapToResponse(ProjectTemplate t) =>
            new ProjectTemplateResponse
            {
                TemplateId = t.TemplateId,
                Name = t.Name,
                MediaType = t.MediaType
            };
    }
}
