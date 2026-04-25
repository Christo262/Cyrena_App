using Cyrena.Contracts;
using Cyrena.Models;
using Microsoft.SemanticKernel;

namespace Cyrena.Services
{
    internal class DefaultAssistantPlugin : IAssistantPlugin
    {
        public string[] Modes => [IAssistantMode.AssistantModeDefault];
        public int Priority => 10;

        public string Id => "cyrena.default";

        public bool Required => true;

        public string Title => "Default Assistant";

        public Task LoadAsync(CyrenaKernelBuilder builder)
        {
            builder.Plugins.AddFromType<Chat>();
            return Task.CompletedTask;
        }
    }
}
