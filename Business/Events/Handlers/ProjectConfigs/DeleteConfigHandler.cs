using DataLabelProject.Business.Events.Abstraction;
using DataLabelProject.Business.Events.DomainEvents.Project;
using DataLabelProject.Data.Repositories.Abstractions;

namespace DataLabelProject.Business.Events.Handlers.ProjectConfigs;

public class DeleteConfigHandler 
    : IEventHandler<ProjectDeletedEvent>
{
    private readonly IProjectConfigRepository _configRepository;

    public DeleteConfigHandler(IProjectConfigRepository configRepository)
    {
        _configRepository = configRepository;
    }

    public async Task HandleAsync(ProjectDeletedEvent domainEvent)
    {
        var config = await _configRepository.GetByProjectIdAsync(domainEvent.ProjectId);
        if (config == null) return;

        await _configRepository.DeleteAsync(config);
        await _configRepository.SaveChangesAsync();
    }
}