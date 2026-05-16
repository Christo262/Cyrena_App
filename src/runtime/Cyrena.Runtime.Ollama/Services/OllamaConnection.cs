using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;
using Newtonsoft.Json;
using Cyrena.Contracts;
using Cyrena.Models;
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

        public OllamaConnection(IIterationService its, IChatMessageService chat, IChatCompletionService completion, OllamaConnectionInfo options, IServiceProvider services)
        {
            _its = its;
            _chat = chat;
            _completion = completion;
            _options = options;
            _lock = new object();
            _services = services;
        }

        private StringBuilder? _responseBuilder { get; set; }

        public async Task HandleAsync(AuthorRole role, string input, Kernel kernel, CancellationToken ct = default)
        {
            _its.InferenceStart();
            await _chat.AddMessage(role, input);
            var settings = CreateExecutionSettings(FunctionChoiceBehavior.Auto());
            await RunInferenceAsync(settings, kernel, ct, handleToolCalls: true);
        }

        public async Task HandleAsync(AuthorRole role, string input, Kernel kernel, CancellationToken ct = default, params AdditionalMessageContent[] items)
        {
            _its.InferenceStart();
            await _chat.AddMessage(role, input, items);
            var settings = CreateExecutionSettings(FunctionChoiceBehavior.Auto());
            await RunInferenceAsync(settings, kernel, ct, handleToolCalls: true);
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

        private async Task RunInferenceAsync(OllamaPromptExecutionSettings settings, Kernel kernel, CancellationToken ct, bool handleToolCalls)
        {
            try
            {
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

                var text = _responseBuilder.ToString();
                _responseBuilder = null;

                if (string.IsNullOrEmpty(text))
                {
                    await _chat.AddMessage(AuthorRole.Assistant, text);
                    return;
                }

                if (!handleToolCalls)
                {
                    await _chat.AddMessage(AuthorRole.Assistant, text);
                    return;
                }

                var json = ExtractJson(text);
                if (string.IsNullOrEmpty(json))
                {
                    await _chat.AddMessage(AuthorRole.Assistant, text);
                    return;
                }

                // Handle a toolcall SemanticKernel may have missed
                await HandleToolCallAsync(text, json, kernel, ct);
            }
            finally
            {
                _its.InferenceEnd();
            }
        }

        private async Task HandleToolCallAsync(string text, string json, Kernel kernel, CancellationToken ct)
        {
            try
            {
                ToolCall? toolCall = null;
                try
                {
                    toolCall = JsonConvert.DeserializeObject<ToolCall>(json);
                }
                catch { }

                if (toolCall == null || toolCall.Name == null)
                {
                    await _chat.AddMessage(AuthorRole.Assistant, text);
                    return;
                }

                KernelFunction? function = null;
                foreach (var plugin in kernel.Plugins)
                {
                    if (plugin.TryGetFunction(toolCall.Name, out function))
                        break;
                }

                if (function == null)
                {
                    await _chat.AddMessage(AuthorRole.Assistant, $"Error: Function '{toolCall.Name}' not found.");
                    return;
                }

                var result = await kernel.InvokeAsync(function, new KernelArguments(toolCall.Arguments ?? toolCall.Parameters ?? new Dictionary<string, object?>()));
                var toolText =
                $"""
                [TOOL_RESULT name="{toolCall.Name}"]
                {result}
                [/TOOL_RESULT]
                """;
                await HandleAsync(AuthorRole.Tool, toolText, kernel, ct);
            }
            catch (Exception ex)
            {
                await _chat.LogError(ex.Message);
            }
        }

        public void FunctionCallStart()
        {
            lock (_lock)
            {
                _responseBuilder?.Clear();
            }
        }

        private string ExtractJson(string text)
        {
            text = text.Trim();

            // Try to extract from markdown code blocks
            var match = System.Text.RegularExpressions.Regex.Match(
                text,
                @"```(?:json)?\s*(\{.*?\})\s*```",
                System.Text.RegularExpressions.RegexOptions.Singleline
            );

            if (match.Success)
            {
                return match.Groups[1].Value.Trim();
            }

            // If no code block, check if the entire text is JSON
            if (text.StartsWith("{") && text.EndsWith("}"))
            {
                return text;
            }

            // Try to find JSON object anywhere in the text
            match = System.Text.RegularExpressions.Regex.Match(
                text,
                @"\{[^{}]*(?:\{[^{}]*\}[^{}]*)*\}",
                System.Text.RegularExpressions.RegexOptions.Singleline
            );

            if (match.Success)
            {
                return match.Value.Trim();
            }

            return string.Empty;
        }

        public sealed record ToolCall
        {
            [JsonProperty("name")]
            public string? Name { get; init; }

            [JsonProperty("arguments")]
            public Dictionary<string, object?>? Arguments { get; init; }

            [JsonProperty("parameters")]
            public Dictionary<string, object?>? Parameters { get; init; }
        }
    }
}
