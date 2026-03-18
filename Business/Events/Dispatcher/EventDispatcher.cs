using DataLabelProject.Business.Events.Abstraction;

namespace DataLabelProject.Business.Events.Dispatcher;

public class EventDispatcher : IEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public EventDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task DispatchAsync<TEvent>(TEvent domainEvent)
        where TEvent : IDomainEvent
    {
        var handlers = _serviceProvider.GetServices<IEventHandler<TEvent>>();

        foreach (var handler in handlers)
        {
            await handler.HandleAsync(domainEvent);
        }
    }
}