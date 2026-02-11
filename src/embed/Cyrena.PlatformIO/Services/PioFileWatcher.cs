using Cyrena.Contracts;
using Cyrena.Events;
using Cyrena.Extensions;
using Cyrena.PlatformIO.Contracts;
using Cyrena.PlatformIO.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Cyrena.PlatformIO.Services
{
    internal class PioFileWatcher :
        IEventHandler<FileCreatedEvent>,
        IEventHandler<FileDeletedEvent>,
        IEventHandler<FileRenamedEvent>
    {
        private readonly IDeveloperContext _context;
        private readonly IEnvironmentController _env;

        public PioFileWatcher(IDeveloperContext context, IEnvironmentController env)
        {
            _context = context;
            _env = env;
        }

        public Task HandleAsync(FileCreatedEvent e, CancellationToken ct = default)
        {
            HandleChange(e.FullPath, created: true);
            return Task.CompletedTask;
        }

        public Task HandleAsync(FileDeletedEvent e, CancellationToken ct = default)
        {
            HandleChange(e.FullPath, created: false);
            return Task.CompletedTask;
        }

        public Task HandleAsync(FileRenamedEvent e, CancellationToken ct = default)
        {
            HandleForceReindex(e.FullPath);
            return Task.CompletedTask;
        }

        private void HandleChange(string fullPath, bool created)
        {
            if (!_context.IsTrackedFile(fullPath))
                return;

            var name = Path.GetFileNameWithoutExtension(fullPath);

            bool exists = _context.ProjectPlan.TryFindFileByName(name, out _);

            // created → reindex if missing
            // deleted → reindex if exists
            if ((created && !exists) || (!created && exists))
            {
                HandleForceReindex(fullPath);
            }
        }

        private void HandleForceReindex(string fullPath)
        {
            _context.ProjectPlan.IndexPlatformIODefaultPlan();

            if (_env.Current!.Framework?
                .Split(',', StringSplitOptions.TrimEntries)
                .Any(f => f.Equals("espidf", StringComparison.OrdinalIgnoreCase)) == true)
            {
                _context.ProjectPlan.IndexPlatformIOEspIdf();
            }

            _context.Kernel.Services
                .GetRequiredService<IDeveloperWindow>()
                .FilesChanged();
        }
    }

}
