using Cyrena.Contracts;
using Cyrena.Models;
using Cyrena.Options;
using Cyrena.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Cyrena.Extensions
{
    public static class DeveloperContextBuilderExtensions
    {
        public static void AddProjectFileWatcher(this IDeveloperContextBuilder builder)
        {
            builder.Services.AddSingleton<ProjectFileWatcher>();
            builder.AddStartupTask(0, (ctx) =>
            {
                var fw = ctx.Kernel.Services.GetRequiredService<ProjectFileWatcher>();
                fw.Start();
                return Task.CompletedTask;
            });
        }

        public static void AddStartupTask(this IDeveloperContextBuilder builder, int order, Func<IDeveloperContext, Task> action)
        {
            builder.Services.AddSingleton<IStartupTask>(sp =>
            {
                var ctx = sp.GetRequiredService<IDeveloperContext>();
                return new StartupTask(order, action, ctx);
            });
        }

        public static void AddStartupTask<TStartupTask>(this IDeveloperContextBuilder builder) where TStartupTask : class, IStartupTask
        {
            builder.Services.AddSingleton<IStartupTask, TStartupTask>();
        }

        public static void AddEventHandler<TEvent, TEventHandler>(this IDeveloperContextBuilder builder)
            where TEvent : class, IEvent
            where TEventHandler : class, IEventHandler<TEvent>
        {
            builder.Services.AddScoped<TEventHandler>();
            var wrapper = new EventHandlerWrapperImpl<TEvent>(typeof(TEventHandler));
            builder.Services.AddSingleton<EventHandlerWrapper>(wrapper);
        }
    }

    internal class StartupTask : IStartupTask
    {
        private readonly Func<IDeveloperContext, Task> _action;
        private readonly IDeveloperContext _context;
        public StartupTask(int order, Func<IDeveloperContext, Task> action, IDeveloperContext context)
        {
            Order = order;
            _action = action;
            _context = context;
        }

        public int Order { get; }

        public Task RunAsync(CancellationToken cancellationToken = default)
        {
            return _action(_context);
        }
    }
}
