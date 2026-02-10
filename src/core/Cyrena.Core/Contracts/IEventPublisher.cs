using Cyrena.Models;

namespace Cyrena.Contracts
{
    public interface IEventPublisher
    {
        void Publish(IEvent e);
    }
}
