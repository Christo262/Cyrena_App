using Cyrena.Blazor.Extensions;
using Cyrena.Contracts;
using Cyrena.Events;
using Microsoft.Extensions.DependencyInjection;

namespace Cyrena.Blazor.Services
{
    internal class BlazorProjectFileWatcher : IEventHandler<FileCreatedEvent>, IEventHandler<FileDeletedEvent>, IEventHandler<FileRenamedEvent>
    {
        private readonly IDeveloperContext _ctx;
        public BlazorProjectFileWatcher(IDeveloperContext ctx)
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
