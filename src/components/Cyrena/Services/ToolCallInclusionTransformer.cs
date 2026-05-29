using Cyrena.Contracts;
using Cyrena.Models;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Cyrena.Services
{
    /// <summary>
    /// Attempts to ensure that tool calls are persisted. Suppresses attachment_get function results to shorten context
    /// </summary>
    internal class ToolCallInclusionTransformer : ConversationHistoryTransformer
    {
        private readonly IChatMessageService _chat;
        public ToolCallInclusionTransformer(IChatMessageService chat)
        {
            _chat = chat;
        }

        public override async Task ApplyPostStreamModification(ChatHistory history)
        {
            var last = history.LastOrDefault(x => x.Role == _chat.Options.User);
            if(last != null)
            {
                var idx = history.IndexOf(last);
                List<string> attachment_call_ids = new List<string>();
                for (int i = idx; i < history.Count; i++)
                {
                    var target = history[i];
                    if(target.Role == _chat.Options.Assistant || target.Role == _chat.Options.Tool)
                    {
                        for(int t = 0; t < target.Items.Count; t++)
                        {
                            var item = target.Items[t];
                            if (item is FunctionCallContent fnc)
                            {
                                if (fnc.FunctionName == "Attachment_get" && !string.IsNullOrEmpty(fnc.Id))
                                    attachment_call_ids.Add(fnc.Id);
                            }
                            if (item is FunctionResultContent fnr)
                            {
                                if (!string.IsNullOrEmpty(fnr.CallId) && attachment_call_ids.Contains(fnr.CallId))
                                {
                                    var suppress = new FunctionResultContent(fnr.FunctionName, fnr.PluginName, fnr.CallId, "[FILE RETURNED: content omitted from history. Re-call Attachment_get if needed.]");
                                    target.Items[t] = suppress;
                                }

                                if(fnr.Result is ISuppressibleResult res)
                                {
                                    var suppress = new FunctionResultContent(fnr.FunctionName, fnr.PluginName, fnr.CallId, res.Suppress());
                                    target.Items[t] = suppress;
                                }
                            }
                        }
                        await _chat.AddMessage(target);
                    }
                }
            }
        }
    }
}
