using DataLabelProject.Business.Events.Abstraction;
using DataLabelProject.Business.Events.DomainEvents.Project;
using DataLabelProject.Business.Models;
using DataLabelProject.Data.Repositories.Abstractions;

namespace DataLabelProject.Business.Events.Handlers.ProjectConfigs;

public class CreateConfigHandler 
    : IEventHandler<ProjectCreatedEvent>
{
    private readonly IProjectConfigRepository _configRepository;

    public CreateConfigHandler(IProjectConfigRepository configRepository)
    {
        _configRepository = configRepository;
    }

    public async Task HandleAsync(ProjectCreatedEvent domainEvent)
    {
        var config = new ProjectConfig
        {
            ProjectId = domainEvent.ProjectId
        };

        await _configRepository.CreateAsync(config);
        await _configRepository.SaveChangesAsync();
    }
}