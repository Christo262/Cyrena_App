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
            builder.Plugins.AddFromType<Cyrena.Runtime.Plugins.DateTime>();
            var config_service = new ChatConfigurationService(_store, config);
            builder.Services.AddSingleton<IChatConfigurationService>(config_service);

            return Task.CompletedTask;
        }
    }
}
