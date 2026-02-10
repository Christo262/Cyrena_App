using Cyrena.Models;

namespace Cyrena.Contracts
{
    public interface IEventHandler<TEvent>
        where TEvent : class, IEvent
    {
        Task HandleAsync(TEvent e, CancellationToken ct = default);
    }
}
