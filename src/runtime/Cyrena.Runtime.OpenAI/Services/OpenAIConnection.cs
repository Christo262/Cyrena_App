using Cyrena.Contracts;
using Cyrena.Runtime.OpenAI.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Text;
using Cyrena.Extensions;

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
        private readonly IKernelResolver _kernel;
        public OpenAIConnection(IIterationService its, IChatMessageService chat, IChatCompletionService completion, OpenAIModel model, IServiceProvider services, IKernelResolver kernel)
        {
            _its = its;
            _chat = chat;
            _completion = completion;
            _model = model;
            _lock = new object();
            _services = services;
            _kernel = kernel;
        }

        private StringBuilder? _responseBuilder { get; set; }
        public void FunctionCallStart()
        {
            lock (_lock)
            {
                _responseBuilder?.Clear();
            }
        }

        public async Task HandleAsync(ChatMessageContent content, CancellationToken ct = default)
        {
            _its.InferenceStart();
            await _chat.AddMessage(content);
            await RunInferenceAsync(ct);
        }

        private async Task RunInferenceAsync(CancellationToken ct)
        {
            try
            {
                var settings = CreateExecutionSettings();
                _responseBuilder = new StringBuilder();
                var history = await _chat.GetKernelHistory();

                await foreach (var chunk in _completion.GetStreamingChatMessageContentsAsync(history, settings, _kernel.Resolve(), ct))
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
            }
            finally
            {
                _its.InferenceEnd();
                _responseBuilder = null;
            }
        }

        private OpenAIPromptExecutionSettings CreateExecutionSettings()
        {
            return new OpenAIPromptExecutionSettings()
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
                Temperature = _model.Temperature,
                TopP = _model.TopP,
            };
        }
    }
}
