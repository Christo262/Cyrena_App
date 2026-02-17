using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Models;
using Cyrena.Persistence.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Cyrena.Runtime.Services
{
    /// <summary>
    /// Registers usage tracker and startup task
    /// </summary>
    internal class AllAssistantsPlugin : IAssistantPlugin
    {
        private readonly IKernelController _store;
        public AllAssistantsPlugin(IKernelController store)
        {
            _store = store;
        }

        public string[] Modes => [];

        public int Priority => 10;

        public Task LoadAsync(ChatConfiguration config, IKernelBuilder builder)
        {
            var usage = new UsageState()
            {
                LastUsed = DateTime.Now,
            };
            builder.Plugins.AddFromType<Cyrena.Runtime.Plugins.DateTime>();
            builder.Services.AddSingleton(usage);
            builder.AddStartupTask<KernelUsageStartupTask>();

            var config_service = new ChatConfigurationService(_store, config);
            builder.Services.AddSingleton<IChatConfigurationService>(config_service);

            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Ensure that activity is being tracked so the <see cref="IKernelController"/> can remove stale instance
    /// </summary>
    internal class KernelUsageStartupTask : IStartupTask
    {
        private readonly UsageState _state;
        private readonly IIterationService _iteration;
        private readonly IChatMessageService _srv;
        public KernelUsageStartupTask(IChatMessageService srv, UsageState state, IIterationService iteration)
        {
            _srv = srv;
            _state = state;
            _iteration = iteration;
        }

        public int Order => 0;

        public Task RunAsync(CancellationToken cancellationToken = default)
        {
            _srv.OnDisplayHistoryChanged(OnHistoryCallback);
            _srv.OnKernelHistoryChanged(OnHistoryCallback);

            _iteration.OnIterationStart((e) => _state.CanDispose = false);
            _iteration.OnIterationEnd((e) => _state.CanDispose = true);
            return Task.CompletedTask;
        }
        private void OnHistoryCallback(ChatHistory hst)
        {
            _state.LastUsed = DateTime.Now;
        }
    }

    internal class UsageState
    {
        public DateTime LastUsed { get; set; }
        public bool CanDispose { get; set; }
    }
}
