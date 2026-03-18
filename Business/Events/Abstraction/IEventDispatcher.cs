namespace DataLabelProject.Business.Events.Abstraction;

public interface IEventDispatcher
{
    Task DispatchAsync<TEvent>(TEvent domainEvent) where TEvent : IDomainEvent;
}