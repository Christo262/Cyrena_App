using Cyrena.ClassLibrary.Extensions;
using Cyrena.Contracts;
using Cyrena.Events;
using Microsoft.Extensions.DependencyInjection;

namespace Cyrena.ClassLibrary.Services
{
    internal class LibProjectFileWatcher : IEventHandler<FileCreatedEvent>, IEventHandler<FileDeletedEvent>, IEventHandler<FileRenamedEvent>
    {
        private readonly IDeveloperContext _ctx;
        public LibProjectFileWatcher(IDeveloperContext ctx)
        {
            _ctx = ctx;
        }

        public Task HandleAsync(FileCreatedEvent e, CancellationToken ct)
        {
            if (_ctx.IsTrackedFile(e.FullPath))
                _ctx.Kernel.Services.GetRequiredService<IDeveloperWindow>().FilesChanged();
            return Task.CompletedTask;
        }

        public Task HandleAsync(FileDeletedEvent e, CancellationToken ct)
        {
            if (_ctx.IsTrackedFile(e.FullPath))
                _ctx.Kernel.Services.GetRequiredService<IDeveloperWindow>().FilesChanged();
            return Task.CompletedTask;
        }

        public Task HandleAsync(FileRenamedEvent e, CancellationToken ct)
        {
            if (_ctx.IsTrackedFile(e.FullPath))
                _ctx.Kernel.Services.GetRequiredService<IDeveloperWindow>().FilesChanged();
            return Task.CompletedTask;
        }
    }
}
