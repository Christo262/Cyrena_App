using Cyrena.Contracts;
using Cyrena.Models;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Text;

namespace Cyrena.Runtime.OpenAI.Services
{
    internal class OpenAIConnection : IConnection
    {
        public async Task HandleAsync(string input, AuthorRole role, IDeveloperContext ws, CancellationToken ct = default)
        {
            ws.HandleStart();
            ws.AddMessage(role, input);
            var chat = ws.Kernel.GetRequiredService<IChatCompletionService>();
            OpenAIPromptExecutionSettings settings = new OpenAIPromptExecutionSettings()
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
            };

            var sb = new StringBuilder();

            await foreach (var chunk in chat.GetStreamingChatMessageContentsAsync(ws.KernelHistory, settings, ws.Kernel, ct))
            {
                var delta = chunk.Content;
                if (string.IsNullOrEmpty(delta)) continue;

                sb.Append(delta);
                ws.OnStreamToken(delta);
            }

            ws.AddMessage(AuthorRole.Assistant, sb.ToString());
            ws.HandleEnd();
            return;
        }
    }
}
