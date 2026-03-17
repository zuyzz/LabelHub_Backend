using DataLabelProject.Business.Events.Abstraction;

namespace DataLabelProject.Business.Events.DomainEvents.Project;

public class ProjectDeletedEvent : IDomainEvent
{
    public Guid ProjectId { get; }

    public ProjectDeletedEvent(Guid projectId)
    {
        ProjectId = projectId;
    }
}