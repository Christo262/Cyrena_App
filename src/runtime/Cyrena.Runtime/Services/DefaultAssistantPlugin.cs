using Cyrena.Contracts;
using Cyrena.Models;
using Cyrena.Runtime.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

namespace Cyrena.Runtime.Services
{
    internal class DefaultAssistantPlugin : IAssistantPlugin
    {
        public string[] Modes => [IAssistantMode.AssistantModeDefault];
        public int Priority => 10;

        public Task LoadAsync(ChatConfiguration config, IKernelBuilder builder)
        {
            builder.Plugins.AddFromType<Chat>();
            return Task.CompletedTask;
        }
    }
}
