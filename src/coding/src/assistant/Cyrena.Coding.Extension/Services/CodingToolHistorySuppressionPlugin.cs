using Cyrena.Coding.Options;
using Cyrena.Contracts;
using Cyrena.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Cyrena.Coding.Services
{
    internal class CodingToolHistorySuppressionPlugin : IAssistantPlugin
    {
        public string Id => "cyrena.code.tool.suppressor";
        public string[] Modes => [DevelopOptions.AssistantModeId];
        public int Priority => 10;
        public bool Required => false;
        public string Title => "Tool History Suppression";

        public Task LoadAsync(CyrenaKernelBuilder builder)
        {
            builder.Services.AddSingleton<IConversationHistoryTransformer, CodingConversationHistoryTransformer>();
            return Task.CompletedTask;
        }
    }
}
