using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;
using Newtonsoft.Json;
using Cyrena.Contracts;
using Cyrena.Models;
using Cyrena.Runtime.Ollama.Models;
using System.Text;

namespace Cyrena.Runtime.Ollama.Services
{
    internal class OllamaConnection : IConnection
    {
        public async Task HandleAsync(string input, AuthorRole role, IDeveloperContext ws, CancellationToken ct = default)
        {
            ws.HandleStart();
            ws.AddMessage(role, input);
            var chat = ws.Kernel.GetRequiredService<IChatCompletionService>();
            var config = ws.Kernel.GetRequiredService<OllamaConnectionInfo>();
            var settings = new OllamaPromptExecutionSettings
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.None(), //Doesnt actually do anything for this release
                Temperature = config.Temperature,
                NumPredict = config.NumPredict,
                ExtensionData = new Dictionary<string, object>(),
                TopK = config.TopK,
                TopP = config.TopP,
                Stop = ["<end/>"]
            };
            settings.ExtensionData["num_ctx"] = config.NumContext;
            settings.ExtensionData["min_p"] = config.MinP;
            if (!string.IsNullOrEmpty(config.Thinking))
                settings.ExtensionData["think"] = config.Thinking;

            var sb = new StringBuilder();

            await foreach (var chunk in chat.GetStreamingChatMessageContentsAsync(ws.KernelHistory, settings, ws.Kernel, ct))
            {
                var delta = chunk.Content;
                if (string.IsNullOrEmpty(delta)) continue;

                sb.Append(delta);
                ws.OnStreamToken(delta);
            }

            var text = sb.ToString();
            if (string.IsNullOrEmpty(text))
            {
                ws.AddMessage(AuthorRole.Assistant, text);
                ws.HandleEnd();
                return;
            }
            var json = ExtractJson(text); //In case the model does not return a OpenAI style tool call response, hopefully this catches it
            if (string.IsNullOrEmpty(json))
            {
                ws.AddMessage(AuthorRole.Assistant, text);
                ws.HandleEnd();
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
                    ws.AddMessage(AuthorRole.Assistant, text);
                    return;
                }
                KernelFunction? function = null;
                foreach (var plugin in ws.Kernel.Plugins)
                {
                    if (plugin.TryGetFunction(toolCall.Name, out function))
                        break;
                }
                if (function == null)
                {
                    ws.AddMessage(AuthorRole.Assistant, $"Error: Function '{toolCall.Name}' not found.");
                    return;
                }
                var result = await ws.Kernel.InvokeAsync(function, new KernelArguments(toolCall.Arguments ?? toolCall.Parameters ?? new Dictionary<string, object?>()));
                var toolText =
                $"""
                [TOOL_RESULT name="{toolCall.Name}"]
                {result}
                [/TOOL_RESULT]
                """;
                await HandleAsync(toolText, AuthorRole.Tool, ws, ct);
            }
            catch (Exception ex)
            {
                ws.LogError(ex.Message);
            }
            finally
            {
                ws.HandleEnd();
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
