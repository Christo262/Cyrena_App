using Cyrena.Contracts;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Cyrena.Services
{
    internal class ToolSuppressionConversationHistoryTransformer : ConversationHistoryTransformer
    {
        private readonly IChatMessageService _chat;
        public ToolSuppressionConversationHistoryTransformer(IChatMessageService chat)
        {
            _chat = chat;
        }

        public override async Task ApplyPostStreamModification(ChatHistory history)
        {
            string? functionName = null;
            foreach (var item in history.Where(x => x.Role == _chat.Options.Tool || x.Role == _chat.Options.Assistant))
            {
                foreach (var i in item.Items)
                {
                    if (i is FunctionCallContent fnc)
                    {
                        functionName = fnc.FunctionName;
                        var text = System.Text.Json.JsonSerializer.Serialize(fnc, new System.Text.Json.JsonSerializerOptions() { WriteIndented = true });
                        await _chat.AddMessage(item.Role, text, true);
                    }
                    if (i is FunctionResultContent fnr)
                    {
                        var fnn = new FunctionResultContent(fnr.FunctionName ?? functionName, fnr.PluginName, fnr.CallId, "[OMITTED for brevity. CALL FUNCTION for updated results]");
                        var tool_text = System.Text.Json.JsonSerializer.Serialize(fnn, new System.Text.Json.JsonSerializerOptions() { WriteIndented = true });
                        await _chat.AddMessage(item.Role, tool_text, true);
                    }
                }
            }
        }
    }
}
