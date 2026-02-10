using Cyrena.Contracts;
using Cyrena.Models;
using Cyrena.Runtime.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Cyrena.Runtime.Services
{
    internal class EventsHostedService : IDisposable
    {
        private readonly EventQueue _queue;
        private readonly IServiceProvider _services;
        private readonly IEnumerable<EventHandlerWrapper> _wrappers;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly List<Task> _currentTasks;
        public EventsHostedService(EventQueue queue, IServiceProvider services)
        {
            _queue = queue;
            _services = services;
            _wrappers = services.GetServices<EventHandlerWrapper>();
            _currentTasks = new List<Task>();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public void Start()
        {
            _ = Task.Run(async () =>
            {
                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    try
                    {
                        _currentTasks.RemoveAll(t => t.IsCompleted);
                        
                        if (_currentTasks.Count < 10)
                        {
                            (bool isE, IEvent? e) = _queue.Read();
                            if (isE && e != null)
                            {
                                var tw = _wrappers.Where(x => x.HandlesType.IsAssignableFrom(e.GetType())).ToList();
                                var task = Task.Run(async () =>
                                {
                                    foreach (var wrap in tw)
                                        try
                                        {
                                            await wrap.Handle(e, _services, _cancellationTokenSource.Token);
                                        }
                                        catch (Exception ex)
                                        {
                                            _services.GetRequiredService<IDeveloperContext>().LogError($"{ex.Message}{Environment.NewLine}{ex.StackTrace}");
                                        }
                                });
                                _currentTasks.Add(task);
                            }
                            else
                                await Task.Delay(10, _cancellationTokenSource.Token);
                        }
                        else
                            await Task.Delay(10, _cancellationTokenSource.Token);
                    }
                    catch (Exception ex)
                    {
                        _services.GetRequiredService<IDeveloperContext>().LogError($"{ex.Message}{Environment.NewLine}{ex.StackTrace}");
                    }
                }
            }, _cancellationTokenSource.Token);
        }
        
        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            //TODO
        }
    }
}
