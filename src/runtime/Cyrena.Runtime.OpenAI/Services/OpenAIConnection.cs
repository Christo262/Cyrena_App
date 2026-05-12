using Cyrena.Contracts;
using Cyrena.Models;
using Cyrena.Runtime.OpenAI.Models;
using Microsoft.Extensions.DependencyInjection;
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
        private readonly OpenAIModel _model;
        private readonly IServiceProvider _services;
        private readonly object _lock;
        public OpenAIConnection(IIterationService its, IChatMessageService chat, IChatCompletionService completion, OpenAIModel model, IServiceProvider services)
        {
            _its = its;
            _chat = chat;
            _completion = completion;
            _model = model;
            _lock = new object();
            _services = services;
        }

        private StringBuilder? _responseBuilder { get; set; }
        public void FunctionCallStart()
        {
            lock (_lock)
            {
                _responseBuilder?.Clear();
            }
        }

        public async Task HandleAsync(AuthorRole role, string input, Kernel kernel, CancellationToken ct = default)
        {
            _its.InferenceStart();
            await _chat.AddMessage(role, input);
            OpenAIPromptExecutionSettings settings = new OpenAIPromptExecutionSettings()
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
                Temperature = _model.Temperature,
                TopP = _model.TopP,
            };

            _responseBuilder = new StringBuilder();
            var history = await _chat.GetKernelHistory();          

            await foreach (var chunk in _completion.GetStreamingChatMessageContentsAsync(history, settings, kernel, ct))
            {
                var delta = chunk.Content;
                if (string.IsNullOrEmpty(delta)) continue;

                lock (_lock)
                {
                    _responseBuilder.Append(delta);
                }
                _chat.Stream(delta);
            }
            var transformers = _services.GetServices<IConversationHistoryTransformer>();
            foreach (var transformer in transformers)
                await transformer.ApplyPostStreamModification(history);

            await _chat.AddMessage(AuthorRole.Assistant, _responseBuilder.ToString());
            _its.InferenceEnd();
            _responseBuilder = null;
            return;
        }

        public async Task HandleAsync(AuthorRole role, string input, Kernel kernel, CancellationToken ct = default, params AdditionalMessageContent[] items)
        {
            _its.InferenceStart();
            await _chat.AddMessage(role, input, items);
            OpenAIPromptExecutionSettings settings = new OpenAIPromptExecutionSettings()
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
                Temperature = _model.Temperature,
                TopP = _model.TopP,
            };

            _responseBuilder = new StringBuilder();
            var history = await _chat.GetKernelHistory();

            await foreach (var chunk in _completion.GetStreamingChatMessageContentsAsync(history, settings, kernel, ct))
            {
                var delta = chunk.Content;
                if (string.IsNullOrEmpty(delta)) continue;

                lock( _lock)
                {
                    _responseBuilder.Append(delta);
                }
                _chat.Stream(delta);
            }

            var transformers = _services.GetServices<IConversationHistoryTransformer>();
            foreach (var transformer in transformers)
                await transformer.ApplyPostStreamModification(history);

            await _chat.AddMessage(AuthorRole.Assistant, _responseBuilder.ToString());
            _its.InferenceEnd();
            _responseBuilder = null;
            return;
        }
    }
}
