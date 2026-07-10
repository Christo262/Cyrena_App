using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;
using Cyrena.Contracts;
using Cyrena.Runtime.Ollama.Models;
using System.Text;
using Cyrena.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Cyrena.Runtime.Ollama.Services
{
    internal class OllamaConnection : IConnection
    {
        private readonly IIterationService _its;
        private readonly IChatMessageService _chat;
        private readonly IChatCompletionService _completion;
        private readonly OllamaConnectionInfo _options;
        private readonly IServiceProvider _services;
        private readonly object _lock;
        private readonly IKernelResolver _kernel;

        public OllamaConnection(IIterationService its, IChatMessageService chat, IChatCompletionService completion, OllamaConnectionInfo options, IServiceProvider services, IKernelResolver kernel)
        {
            _its = its;
            _chat = chat;
            _completion = completion;
            _options = options;
            _lock = new object();
            _services = services;
            _kernel = kernel;
        }

        private StringBuilder? _responseBuilder { get; set; }

        public async Task HandleAsync(ChatMessageContent content, CancellationToken ct = default)
        {
            _its.InferenceStart();
            await _chat.AddMessage(content);
            var settings = CreateExecutionSettings(FunctionChoiceBehavior.Auto());
            await RunInferenceAsync(settings, ct);
        }

        private OllamaPromptExecutionSettings CreateExecutionSettings(FunctionChoiceBehavior functionChoiceBehavior)
        {
            var settings = new OllamaPromptExecutionSettings
            {
                FunctionChoiceBehavior = functionChoiceBehavior,
                Temperature = _options.Temperature,
                ExtensionData = new Dictionary<string, object>(),
                TopK = _options.TopK,
                TopP = _options.TopP,
                Stop = ["<end/>"],
            };
            settings.ExtensionData["num_ctx"] = _options.NumContext;
            settings.ExtensionData["min_p"] = _options.MinP;
            settings.ExtensionData["num_predict"] = _options.NumPredict;
            if (!string.IsNullOrEmpty(_options.Thinking))
                settings.ExtensionData["think"] = _options.Thinking;

            return settings;
        }

        private async Task RunInferenceAsync(OllamaPromptExecutionSettings settings, CancellationToken ct)
        {
            try
            {
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

                var text = _responseBuilder.ToString();
                _responseBuilder = null;

                if (string.IsNullOrEmpty(text))
                {
                    await _chat.AddMessage(AuthorRole.Assistant, "[Empty Response]");
                    return;
                }

                await _chat.AddMessage(AuthorRole.Assistant, text);
            }
            finally
            {
                _its.InferenceEnd();
            }
        }    

        public void FunctionCallStart()
        {
            lock (_lock)
            {
                _responseBuilder?.Clear();
            }
        }
    }
}
