using Cyrena.ArduinoIDE.Extensions;
using Cyrena.Contracts;
using Cyrena.Events;
using Cyrena.Extensions;
using Cyrena.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Cyrena.ArduinoIDE.Services
{
    internal class ArduinoProjectFileWatcher : IEventHandler<FileCreatedEvent>, IEventHandler<FileDeletedEvent>, IEventHandler<FileRenamedEvent>
    {
        private readonly IDeveloperContext _ctx;
        public ArduinoProjectFileWatcher(IDeveloperContext ctx)
        {
            _ctx = ctx;
        }

        public Task HandleAsync(FileCreatedEvent e, CancellationToken ct = default)
        {
            if (_ctx.IsTrackedFile(e.FullPath))
            {
                var name = Path.GetFileNameWithoutExtension(e.FullPath);
                if (!_ctx.ProjectPlan.Files.Any(f => f.Name == name))
                    IndexPlan(_ctx.ProjectPlan);
                _ctx.Kernel.Services.GetRequiredService<IDeveloperWindow>().FilesChanged();
            }
            return Task.CompletedTask;
        }

        public Task HandleAsync(FileDeletedEvent e, CancellationToken ct = default)
        {
            if (_ctx.IsTrackedFile(e.FullPath))
            {
                var name = Path.GetFileNameWithoutExtension(e.FullPath);
                if (!_ctx.ProjectPlan.Files.Any(f => f.Name == name))
                    IndexPlan(_ctx.ProjectPlan);
                _ctx.Kernel.Services.GetRequiredService<IDeveloperWindow>().FilesChanged();
            }
            return Task.CompletedTask;
        }

        public Task HandleAsync(FileRenamedEvent e, CancellationToken ct = default)
        {
            if (_ctx.IsTrackedFile(e.FullPath))
            {
                var name = Path.GetFileNameWithoutExtension(e.FullPath);
                if (!_ctx.ProjectPlan.Files.Any(f => f.Name == name))
                    IndexPlan(_ctx.ProjectPlan);
                _ctx.Kernel.Services.GetRequiredService<IDeveloperWindow>().FilesChanged();
            }
            return Task.CompletedTask;
        }

        private void IndexPlan(ProjectPlan plan)
        {
            plan.IndexFiles("ino", "ino_");
            plan.IndexFiles("cpp", "cpp_");
            plan.IndexFiles("h", "h_");
            ProjectPlan.Save(plan);
        }
    }
}
