using Cyrena.Contracts;
using Cyrena.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Cyrena.Services
{
    internal class ToolHistorySuppressionPlugin : IAssistantPlugin
    {
        public string Id => "cyrena.toolcall.supression";
        public string[] Modes => [IAssistantMode.AssistantModeDefault];
        public int Priority => 10;
        public bool Required => false;

        public string Title => "Tool History Suppression";

        public Task LoadAsync(CyrenaKernelBuilder builder)
        {
            builder.Services.AddSingleton<IConversationHistoryTransformer, ToolSuppressionConversationHistoryTransformer>();
            return Task.CompletedTask;
        }
    }
}
