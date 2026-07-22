using Cyrena.Contracts;
using Cyrena.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Cyrena.Services
{
    internal class ToolCallInclusionPlugin : IAssistantPlugin
    {
        public string Id => "toolcall.persistence";
        public string[] Modes => [];
        public int Priority => 10;
        public bool Required => false;
        public string Title => "Tool History Persistence";

        public Task LoadAsync(CyrenaKernelBuilder builder)
        {
            builder.Services.AddSingleton<IConversationHistoryTransformer, ToolCallInclusionTransformer>();
            return Task.CompletedTask;
        }
    }
}
