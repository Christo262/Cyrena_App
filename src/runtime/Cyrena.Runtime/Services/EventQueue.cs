using Cyrena.Models;
using System.Collections.Concurrent;

namespace Cyrena.Runtime.Services
{
    internal class EventQueue
    {
        private readonly ConcurrentQueue<IEvent> _queue;
        public EventQueue()
        {
            _queue = new ConcurrentQueue<IEvent>();
        }

        public void Queue(IEvent @event)
        {
            _queue.Enqueue(@event);
        }

        public (bool, IEvent? @event) Read()
        {
            bool inq = _queue.TryDequeue(out var @event);
            return (inq, @event);
        }
    }
}
