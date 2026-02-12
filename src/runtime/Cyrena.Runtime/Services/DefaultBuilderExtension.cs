using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Cyrena.Contracts;
using Cyrena.Persistence.Contracts;
using Cyrena.Runtime.Models;
using Cyrena.Runtime.Plugins;
using Cyrena.Extensions;

namespace Cyrena.Runtime.Services
{
    internal class DefaultBuilderExtension : IDeveloperContextExtension
    {
        public int Priority => 0;

        public Task ExtendAsync(IDeveloperContextBuilder builder)
        {
            builder.AddStore<Note>("project_notes");
            builder.Plugins.AddFromType<ProjectInformation>();
            builder.Plugins.AddFromType<FileActions>();
            builder.Plugins.AddFromType<ProjectNotes>();
            builder.Plugins.AddFromType<Plugins.DateTime>();

            builder.Services.AddSingleton<EventQueue>();
            builder.Services.AddSingleton<IEventPublisher, EventPublisher>();
            builder.Services.AddSingleton<EventsHostedService>();

            builder.AddStartupTask(1, (c) =>
            {
                var hs = c.Kernel.GetRequiredService<EventsHostedService>();
                hs.Start();
                return Task.CompletedTask;
            });

            return Task.CompletedTask;
        }
    }
}
