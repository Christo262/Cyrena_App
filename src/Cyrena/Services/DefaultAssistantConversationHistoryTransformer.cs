using Cyrena.Contracts;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Cyrena.Services
{
    internal class DefaultAssistantConversationHistoryTransformer : ConversationHistoryTransformer
    {
        private readonly IChatMessageService _chat;
        public DefaultAssistantConversationHistoryTransformer(IChatMessageService chat)
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
                        functionName = fnc.FunctionName;
                    if (i is FunctionResultContent fnr)
                    {
                        var tool_text = $"[FUNCTION={functionName ?? "unknown"} CALL_ID={fnr.CallId} RESULT OMITTED FOR BREVITY]";
                        await _chat.AddMessage(item.Role, tool_text);
                    }
                }
            }
        }
    }
}
