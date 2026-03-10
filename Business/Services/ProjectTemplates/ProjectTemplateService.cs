using DataLabelProject.Application.DTOs.ProjectTemplate;
using DataLabelProject.Business.Models;
using DataLabelProject.Data.Repositories.Abstractions;

namespace DataLabelProject.Business.Services.ProjectTemplates;

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

    public async Task<ProjectTemplateResponse> CreateAsync(CreateProjectTemplateRequest request)
    {
        var template = new ProjectTemplate
        {
            TemplateId = Guid.NewGuid(),
            Name = request.Name,
            MediaType = request.MediaType
        };

        await _repository.CreateAsync(template);
        await _repository.SaveChangesAsync();

        return MapToResponse(template);
    }

    public async Task<ProjectTemplateResponse?> UpdateAsync(Guid id, UpdateProjectTemplateRequest request)
    {
        var template = await _repository.GetByIdAsync(id);
        if (template == null) return null;

        if (!string.IsNullOrWhiteSpace(request.Name))
            template.Name = request.Name;
        if (request.MediaType.HasValue)
            template.MediaType = request.MediaType.Value;

        await _repository.UpdateAsync(template);
        await _repository.SaveChangesAsync();

        return MapToResponse(template);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var template = await _repository.GetByIdAsync(id);
        if (template == null) return false;

        await _repository.DeleteAsync(template);
        await _repository.SaveChangesAsync();

        return true;
    }

    private static ProjectTemplateResponse MapToResponse(ProjectTemplate t)
    {
        return new ProjectTemplateResponse
        {
            TemplateId = t.TemplateId,
            Name = t.Name,
            MediaType = t.MediaType.ToString()
        };
    }
}
