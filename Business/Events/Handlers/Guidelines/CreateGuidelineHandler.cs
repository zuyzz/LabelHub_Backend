using DataLabelProject.Business.Events.Abstraction;
using DataLabelProject.Business.Events.DomainEvents.Project;
using DataLabelProject.Business.Models;
using DataLabelProject.Data.Repositories.Abstractions;

namespace DataLabelProject.Business.Events.Handlers.Guidelines;

public class CreateGuidelineHandler 
    : IEventHandler<ProjectCreatedEvent>
{
    private readonly IGuidelineRepository _guidelineRepository;

    public CreateGuidelineHandler(IGuidelineRepository guidelineRepository)
    {
        _guidelineRepository = guidelineRepository;
    }

    public async Task HandleAsync(ProjectCreatedEvent domainEvent)
    {
        var guideline = new Guideline
        {
            Content = "",
            CreatedAt = DateTime.UtcNow,
            ProjectId = domainEvent.ProjectId
        };

        await _guidelineRepository.CreateAsync(guideline);
        await _guidelineRepository.SaveChangesAsync();
    }
}