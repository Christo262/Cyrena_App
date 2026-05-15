using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.LTM.Contracts;
using Cyrena.LTM.Models;
using Cyrena.LTM.Options;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text;
using System.Text.RegularExpressions;

namespace Cyrena.LTM.Services
{
    /// <summary>
    /// Startup task that automatically injects relevant long-term memories into the AI's prompt
    /// at the start of each conversation iteration. Uses IPromptManager to add/update a dynamic
    /// prompt containing contextual memories based on the latest user message in the chat history.
    /// </summary>
    internal class MemoryContextInjectionTask : ConversationHistoryTransformer
    {
        private readonly IMemoryService _memoryService;
        private readonly IChatMessageService _chatMessageService;
        private readonly MemoryContextOptions _options;
        private readonly ILogger<MemoryContextInjectionTask> _logger;

        public int Order => 20;

        public MemoryContextInjectionTask(
            IMemoryService memoryService,
            IChatMessageService chatMessageService,
            MemoryContextOptions options,
            ILogger<MemoryContextInjectionTask> logger)
        {
            _memoryService = memoryService ?? throw new ArgumentNullException(nameof(memoryService));
            _chatMessageService = chatMessageService ?? throw new ArgumentNullException(nameof(chatMessageService));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override async Task<ChatHistory> TransformPreIterationHistory(ChatHistory history)
        {
            var latestMessage = history.LastOrDefault();
            if(latestMessage != null && latestMessage.Role == _chatMessageService.Options.User)
            {
                try
                {
                    var options = _options;
                    if (!options.Enabled)
                        return history;

                    if (string.IsNullOrWhiteSpace(latestMessage.Content))
                        return history;

                    // Extract keywords from the latest user message
                    var keywords = ExtractKeywords(latestMessage.Content);
                    if (keywords.Count == 0)
                        return history;

                    // Search for relevant memories
                    var searchOptions = new MemorySearchOptions
                    {
                        Keywords = keywords.ToArray(),
                        MaxResults = options.MaxMemoriesToInject,
                        MinRelevance = options.MinRelevanceThreshold
                    };

                    var searchResults = await _memoryService.SearchAsync(searchOptions);

                    // Filter by max age if configured
                    var filteredResults = searchResults.ToList();
                    if (options.MaxAgeDays.HasValue)
                    {
                        var cutoff = DateTime.UtcNow.AddDays(-options.MaxAgeDays.Value);
                        filteredResults = filteredResults
                            .Where(r => Ulid.Parse(r.Entry.Id).Time >= cutoff)
                            .ToList();
                    }

                    if (filteredResults.Count == 0)
                        return history;

                    // Format memories into a compact context block
                    var contextText = FormatMemoryContext(filteredResults, options);
                    history.Insert(0, new ChatMessageContent(_chatMessageService.Options.System, contextText));
                }catch (Exception ex)
                {
                    await _chatMessageService.LogError(ex.Message);
                }
            }
            return history;
        }

        /// <summary>
        /// Extracts search keywords from a user message. Uses a simple word-split approach
        /// filtering out common stop words and short tokens.
        /// </summary>
        private static readonly Regex _keywordRegex =
    new(@"\b[a-z0-9]+\b", RegexOptions.Compiled);
        private static List<string> ExtractKeywords(string? message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return new List<string>();

            var stopWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "the", "a", "an", "is", "are", "was", "were", "be", "been", "being",
                "have", "has", "had", "do", "does", "did", "will", "would", "could", "should",
                "may", "might", "must", "shall", "can", "need", "dare", "ought", "used",
                "to", "of", "in", "for", "on", "with", "at", "by", "from", "as", "into",
                "through", "during", "before", "after", "above", "below", "between", "under",
                "and", "but", "or", "yet", "so", "if", "because", "although", "though",
                "while", "where", "when", "that", "which", "who", "whom", "whose", "what",
                "this", "these", "those", "i", "you", "he", "she", "it", "we", "they",
                "me", "him", "her", "us", "them", "my", "your", "his", "its", "our", "their"
            };

            return _keywordRegex.Matches(message.ToLowerInvariant())
                    .Select(m => m.Value)
                    .Where(w =>
                        w.Length >= 3 &&
                        !stopWords.Contains(w) &&
                        !w.All(char.IsDigit))
                    .Distinct()
                    .ToList();
        }

        /// <summary>
        /// Formats search results into a compact text block suitable for prompt injection.
        /// </summary>
        private static string FormatMemoryContext(List<MemorySearchResult> results, MemoryContextOptions options)
        {
            var sb = new StringBuilder();
            sb.AppendLine("## Relevant Context from Long-Term Memory");
            sb.AppendLine();

            foreach (var result in results)
            {
                var entry = result.Entry;
                sb.AppendLine($"- **{entry.Title}** (relevance: {result.RelevanceScore:F2})");

                if (!string.IsNullOrWhiteSpace(entry.Description))
                {
                    var desc = entry.Description.Length > 200
                        ? entry.Description.Substring(0, 200) + "..."
                        : entry.Description;
                    sb.AppendLine($"  {desc}");
                }

                if (options.IncludeFacts && entry.Facts?.Count > 0)
                {
                    var factsToShow = entry.Facts.Take(options.MaxFactsPerMemory).ToList();
                    foreach (var fact in factsToShow)
                    {
                        var props = string.Join(", ", fact.Keys.Select(k => $"{k}={fact[k]}"));
                        sb.AppendLine($"  - [{fact.FactType}]: {props}");
                    }
                }

                sb.AppendLine();
            }

            sb.AppendLine("Use the above context to inform your response when relevant.");

            return sb.ToString();
        }
    }
}
