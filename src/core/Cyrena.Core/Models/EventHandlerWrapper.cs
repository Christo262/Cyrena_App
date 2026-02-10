using Cyrena.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Cyrena.Models
{
    public abstract class EventHandlerWrapper
    {
        public abstract Type HandlesType { get; }
        public abstract Type ServiceType { get; }
        public abstract Task Handle(IEvent @event, IServiceProvider services, CancellationToken cancellationToken);
    }

    public class EventHandlerWrapperImpl<TEvent> : EventHandlerWrapper
        where TEvent : class, IEvent
    {
        public EventHandlerWrapperImpl(Type serviceType)
        {
            ServiceType = serviceType;
        }

        public override Type HandlesType => typeof(TEvent);
        public override Type ServiceType { get; }
        public override async Task Handle(IEvent @event, IServiceProvider services, CancellationToken cancellationToken)
        {
            using var sp = services.CreateScope();
            var impl = sp.ServiceProvider.GetService(ServiceType);
            if (impl is IEventHandler<TEvent> he)
                await he.HandleAsync((TEvent)@event, cancellationToken);
        }
    }
}
