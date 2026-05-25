using Cyrena.Contracts;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cyrena.Runtime.Services
{
    internal class HistoryInclusionTransformer : ConversationHistoryTransformer
    {
        private readonly IChatConfigurationService _config;
        private readonly IChatMessageService _chat;
        public HistoryInclusionTransformer(IChatConfigurationService config, IChatMessageService chat)
        {
            _config = config;
            _chat = chat;
        }

        public override Task<ChatHistory> TransformPreIterationHistory(ChatHistory history)
        {
            switch (_config.Config.HistoryInclusion)
            {
                case Cyrena.Models.HistoryInclusionMode.All:
                    return Task.FromResult(history);
                case Cyrena.Models.HistoryInclusionMode.LastTwo:
                    return Task.FromResult(IterationCountHistory(history, 2));
                case Cyrena.Models.HistoryInclusionMode.LastTen:
                    return Task.FromResult(IterationCountHistory(history, 10));
                case Cyrena.Models.HistoryInclusionMode.Instruct:
                    var hst = new ChatHistory();
                    hst.Add(history.Last());
                    return Task.FromResult(hst);
                default:
                    return Task.FromResult(history);
            }
        }

        private ChatHistory IterationCountHistory(ChatHistory history, int completedIterationsToKeep)
        {
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
                .TakeLast(completedIterationsToKeep);

            var selected = completedGroups
                .Concat(latestGroup is not null && latestGroup.Last().Role != _chat.Options.Assistant
                    ? [latestGroup]
                    : [])
                .SelectMany(x => x);

            return new ChatHistory(selected);
        }
    }
}
