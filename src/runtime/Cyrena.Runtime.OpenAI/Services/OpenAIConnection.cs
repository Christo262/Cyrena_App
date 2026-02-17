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
        private readonly IIterationService _its;
        private readonly IChatMessageService _chat;
        private readonly IChatCompletionService _completion;
        public OpenAIConnection(IIterationService its, IChatMessageService chat, IChatCompletionService completion)
        {
            _its = its;
            _chat = chat;
            _completion = completion;
        }

        public async Task HandleAsync(AuthorRole role, string input, Kernel kernel, CancellationToken ct = default)
        {
            _its.InferenceStart();
            await _chat.AddMessage(role, input);
            OpenAIPromptExecutionSettings settings = new OpenAIPromptExecutionSettings()
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
            };

            var sb = new StringBuilder();

            await foreach (var chunk in _completion.GetStreamingChatMessageContentsAsync(_chat.GetKernelHistory(), settings, kernel, ct))
            {
                var delta = chunk.Content;
                if (string.IsNullOrEmpty(delta)) continue;

                sb.Append(delta);
                _chat.Stream(delta);
            }

            await _chat.AddMessage(AuthorRole.Assistant, sb.ToString());
            _its.InferenceEnd();
            return;
        }

        public async Task HandleAsync(AuthorRole role, string input, Kernel kernel, CancellationToken ct = default, params AdditionalMessageContent[] items)
        {
            _its.InferenceStart();
            await _chat.AddMessage(role, input, items);
            OpenAIPromptExecutionSettings settings = new OpenAIPromptExecutionSettings()
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
            };

            var sb = new StringBuilder();

            await foreach (var chunk in _completion.GetStreamingChatMessageContentsAsync(_chat.GetKernelHistory(), settings, kernel, ct))
            {
                var delta = chunk.Content;
                if (string.IsNullOrEmpty(delta)) continue;

                sb.Append(delta);
                _chat.Stream(delta);
            }

            await _chat.AddMessage(AuthorRole.Assistant, sb.ToString());
            _its.InferenceEnd();
            return;
        }
    }
}
