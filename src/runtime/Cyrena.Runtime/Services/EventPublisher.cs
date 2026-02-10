using Cyrena.Contracts;
using Cyrena.Models;

namespace Cyrena.Runtime.Services
{
    internal class EventPublisher : IEventPublisher
    {
        private readonly EventQueue _queue;
        public EventPublisher(EventQueue queue)
        {
            _queue = queue;
        }

        public void Publish(IEvent e)
        {
            _queue.Queue(e);
        }
    }
}
