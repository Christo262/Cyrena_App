using Cyrena.Contracts;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Cyrena.Coding.Services
{
    internal class CodingConversationHistoryTransformer : ConversationHistoryTransformer
    {
        private readonly IChatMessageService _chat;
        public CodingConversationHistoryTransformer(IChatMessageService chat)
        {
            _chat = chat;
        }

        public override Task<ChatHistory> TransformPreIterationHistory(ChatHistory history)
        {
            const int CompletedIterationsToKeep = 2;

            var groups = new List<List<ChatMessageContent>>();
            List<ChatMessageContent>? current = null;

            foreach (var message in history)
            {
                if (message.Role == _chat.Options.User)
                {
                    current = new List<ChatMessageContent>();
                    groups.Add(current);
                }

                current?.Add(message);
            }

            var latestGroup = groups.LastOrDefault();

            var completedGroups = groups
                .Where(group =>
                    group.Count > 0 &&
                    group.First().Role == _chat.Options.User &&
                    group.Last().Role == _chat.Options.Assistant)
                .TakeLast(CompletedIterationsToKeep);

            var selected = completedGroups
                .Concat(latestGroup is not null && latestGroup.Last().Role != _chat.Options.Assistant
                    ? [latestGroup]
                    : [])
                .SelectMany(x => x);

            return Task.FromResult(new ChatHistory(selected));
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
