using Cyrena.Contracts;
using Cyrena.Events;
using Cyrena.Options;

namespace Cyrena.Services
{
    internal class ProjectFileWatcher : IDisposable
    {
        private readonly FileSystemWatcher _watcher;
        private readonly IDeveloperContext _context;
        private readonly IEventPublisher _publisher;

        private CancellationTokenSource? _debounceCts;
        private readonly object _debounceLock = new();

        public ProjectFileWatcher(IDeveloperContext context, IEventPublisher publisher)
        {
            _context = context;
            _watcher = new FileSystemWatcher(context.Project.RootDirectory, "*")
            {
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName,
                InternalBufferSize = 64 * 1024
            };

            _watcher.Created += OnCreated;
            _watcher.Deleted += OnDeleted;
            _watcher.Renamed += OnRenamed;
            _watcher.Error += OnError;
            _publisher = publisher;
        }

        public void Start()
        {
            _watcher.EnableRaisingEvents = true;
        }

        private void Debounce(Action action)
        {
            lock (_debounceLock)
            {
                _debounceCts?.Cancel();
                _debounceCts = new CancellationTokenSource();
                var token = _debounceCts.Token;

                Task.Run(async () =>
                {
                    try
                    {
                        await Task.Delay(500, token);
                        action();
                    }
                    catch (TaskCanceledException)
                    {
                        // expected when events arrive quickly
                    }
                }, token);
            }
        }

        private void OnCreated(object sender, FileSystemEventArgs e)
        {
            var ev = new FileCreatedEvent(e.FullPath, e.Name, e.ChangeType);
            Debounce(() => _publisher.Publish(ev));
        }

        private void OnDeleted(object sender, FileSystemEventArgs e)
        {
            var ev = new FileDeletedEvent(e.FullPath, e.Name, e.ChangeType);
            Debounce(() => _publisher.Publish(ev));
        }

        private void OnRenamed(object sender, RenamedEventArgs e)
        {
            var ev = new FileRenamedEvent(e.FullPath, e.Name, e.ChangeType, e.OldFullPath, e.OldName);
            Debounce(() => _publisher.Publish(ev));
        }

        private void OnError(object sender, ErrorEventArgs e)
            => _context.LogError(e.GetException().Message);

        public void Dispose()
        {
            _watcher.EnableRaisingEvents = false;
            _debounceCts?.Cancel();

            _watcher.Created -= OnCreated;
            _watcher.Deleted -= OnDeleted;
            _watcher.Renamed -= OnRenamed;
            _watcher.Error -= OnError;
            _watcher.Dispose();
        }
    }
}
