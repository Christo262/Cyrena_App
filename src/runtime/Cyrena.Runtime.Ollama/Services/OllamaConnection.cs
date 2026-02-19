using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;
using Newtonsoft.Json;
using Cyrena.Contracts;
using Cyrena.Models;
using Cyrena.Runtime.Ollama.Models;
using System.Text;
using Cyrena.Extensions;

namespace Cyrena.Runtime.Ollama.Services
{
    internal class OllamaConnection : IConnection
    {
        private readonly IIterationService _its;
        private readonly IChatMessageService _chat;
        private readonly IChatCompletionService _completion;
        private readonly OllamaConnectionInfo _options;
        public OllamaConnection(IIterationService its, IChatMessageService chat, IChatCompletionService completion, OllamaConnectionInfo options)
        {
            _its = its;
            _chat = chat;
            _completion = completion;
            _options = options;
        }

        public async Task HandleAsync(AuthorRole role, string input, Kernel kernel, CancellationToken ct = default)
        {
            _its.InferenceStart();
            await _chat.AddMessage(role, input);
            var settings = new OllamaPromptExecutionSettings
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.None(), //Doesnt actually do anything for this release
                Temperature = _options.Temperature,
                NumPredict = _options.NumPredict,
                ExtensionData = new Dictionary<string, object>(),
                TopK = _options.TopK,
                TopP = _options.TopP,
                Stop = ["<end/>"]
            };
            settings.ExtensionData["num_ctx"] = _options.NumContext;
            settings.ExtensionData["min_p"] = _options.MinP;
            if (!string.IsNullOrEmpty(_options.Thinking))
                settings.ExtensionData["think"] = _options.Thinking;

            var sb = new StringBuilder();

            await foreach (var chunk in _completion.GetStreamingChatMessageContentsAsync(_chat.GetKernelHistory(), settings, kernel, ct))
            {
                var delta = chunk.Content;
                if (string.IsNullOrEmpty(delta)) continue;

                sb.Append(delta);
                _chat.Stream(delta);
            }

            var text = sb.ToString();
            if (string.IsNullOrEmpty(text))
            {
                await _chat.AddMessage(AuthorRole.Assistant, text);
                _its.InferenceEnd();
                return;
            }
            var json = ExtractJson(text); //In case the model does not return a OpenAI style tool call response, hopefully this catches it
            if (string.IsNullOrEmpty(json))
            {
                await _chat.AddMessage(AuthorRole.Assistant, text);
                _its.InferenceEnd();
                return;
            }

            //Handle a toolcall SemanticKernel may have missed
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
            finally
            {
                _its.InferenceEnd();
            }
        }

        public async Task HandleAsync(AuthorRole role, string input, Kernel kernel, CancellationToken ct = default, params AdditionalMessageContent[] items)
        {
            _its.InferenceStart();
            await _chat.AddMessage(role, input, items);
            var settings = new OllamaPromptExecutionSettings
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.None(), //Doesnt actually do anything for this release
                Temperature = _options.Temperature,
                NumPredict = _options.NumPredict,
                ExtensionData = new Dictionary<string, object>(),
                TopK = _options.TopK,
                TopP = _options.TopP,
                Stop = ["<end/>"]
            };
            settings.ExtensionData["num_ctx"] = _options.NumContext;
            settings.ExtensionData["min_p"] = _options.MinP;
            if (!string.IsNullOrEmpty(_options.Thinking))
                settings.ExtensionData["think"] = _options.Thinking;

            var sb = new StringBuilder();

            await foreach (var chunk in _completion.GetStreamingChatMessageContentsAsync(_chat.GetKernelHistory(), settings, kernel, ct))
            {
                var delta = chunk.Content;
                if (string.IsNullOrEmpty(delta)) continue;

                sb.Append(delta);
                _chat.Stream(delta);
            }

            var text = sb.ToString();
            if (string.IsNullOrEmpty(text))
            {
                await _chat.AddMessage(AuthorRole.Assistant, text);
                _its.InferenceEnd();
                return;
            }
            var json = ExtractJson(text); //In case the model does not return a OpenAI style tool call response, hopefully this catches it
            if (string.IsNullOrEmpty(json))
            {
                await _chat.AddMessage(AuthorRole.Assistant, text);
                _its.InferenceEnd();
                return;
            }

            //Handle a toolcall SemanticKernel may have missed
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
            finally
            {
                _its.InferenceEnd();
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

        public sealed class ToolCall
        {
            [JsonProperty("name")]
            public string? Name { get; set; }
            [JsonProperty("arguments")]
            public Dictionary<string, object?>? Arguments { get; set; }
            [JsonProperty("parameters")]
            public Dictionary<string, object?>? Parameters { get; set; }
        }
    }
}
