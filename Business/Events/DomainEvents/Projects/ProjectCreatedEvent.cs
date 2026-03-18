using DataLabelProject.Business.Events.Abstraction;

namespace DataLabelProject.Business.Events.DomainEvents.Project;

public class ProjectCreatedEvent : IDomainEvent
{
    public Guid ProjectId { get; }

    public ProjectCreatedEvent(Guid projectId)
    {
        ProjectId = projectId;
    }
}