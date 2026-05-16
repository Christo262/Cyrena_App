using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.LTM.Contracts;
using Cyrena.LTM.Models;
using Cyrena.Models;
using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Text;

namespace Cyrena.LTM.Services
{
    internal class MemoryAssistantKernelFunctions
    {
        private readonly IMemoryService _ltm;
        private readonly IChatMessageService _chat;

        public MemoryAssistantKernelFunctions(IMemoryService ltm, IChatMessageService chat)
        {
            _ltm = ltm;
            _chat = chat;
        }

        // ------------------------------------------------------------------
        // Helper: resolve or auto-create a category by name
        // ------------------------------------------------------------------
        private async Task<Category> ResolveCategoryAsync(
            string category_name,
            CategoryDecay? decay_if_new,
            CancellationToken ct)
        {
            var existing = await _ltm.GetCategoryByNameAsync(category_name, ct);
            if (existing is not null)
                return existing;

            // Category does not exist — create it with the requested decay
            var decay = decay_if_new ?? CategoryDecay.Normal;
            await _chat.LogInfo($"Auto-creating memory category '{category_name}' with decay '{decay}'...");
            return await _ltm.CreateCategoryAsync(category_name, null, decay, ct);
        }

        // ------------------------------------------------------------------
        // 1. remember
        // ------------------------------------------------------------------
        [KernelFunction("remember")]
        [Description(
            "Store a new memory (fact) in long-term memory. " +
            "Provide the category name, a descriptive title, and the fact details as key-value pairs. " +
            "If the category does not exist, it is created automatically using the specified decay rate. " +
            "Returns the memory ID so you can reference it later.")]
        public async Task<ToolResult> Remember(
            [Description("Name of the category to store this memory in (e.g. 'user_preferences', 'project_context', 'facts'). If it does not exist, it will be created.")] string category_name,
            [Description("Short, descriptive title of the memory (e.g. 'User likes dark theme').")] string title,
            [Description("The fact type — a free-form label (e.g. 'preference', 'event', 'skill', 'relationship', 'goal').")] string fact_type,
            [Description("First property key. Required. Example: 'subject'.")] string key1,
            [Description("First property value. Required. Example: 'editor_theme'.")] string value1,
            [Description("Optional second property key.")] string? key2 = null,
            [Description("Optional second property value.")] string? value2 = null,
            [Description("Optional third property key.")] string? key3 = null,
            [Description("Optional third property value.")] string? value3 = null,
            [Description("Optional fourth property key.")] string? key4 = null,
            [Description("Optional fourth property value.")] string? value4 = null,
            [Description("Optional fifth property key.")] string? key5 = null,
            [Description("Optional fifth property value.")] string? value5 = null,
            [Description("Optional longer description or context for the memory.")] string? description = null,
            [Description("Keywords for future search. Provide 3-8 relevant terms.")] string[]? keywords = null,
            [Description("Decay rate if the category needs to be created: Fast (7 days), Normal (30 days), Slow (90 days), None (never). Default: Normal.")] string? decay = null,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(category_name))
                return new ToolResult(false, "category_name is required.");
            if (string.IsNullOrWhiteSpace(title))
                return new ToolResult(false, "Title is required.");
            if (string.IsNullOrWhiteSpace(fact_type))
                return new ToolResult(false, "fact_type is required.");
            if (string.IsNullOrWhiteSpace(key1))
                return new ToolResult(false, "key1 is required.");

            // Parse decay string to enum
            CategoryDecay? decayIfNew = null;
            if (!string.IsNullOrWhiteSpace(decay))
            {
                if (!Enum.TryParse<CategoryDecay>(decay, true, out var parsedDecay))
                    return new ToolResult(false, $"Invalid decay value '{decay}'. Use: Fast, Normal, Slow, or None.");
                decayIfNew = parsedDecay;
            }

            // Resolve category (find by name or auto-create)
            var category = await ResolveCategoryAsync(category_name, decayIfNew, ct);

            // Create the memory entry
            await _chat.LogInfo($"Remembering: {title} in category '{category.Name}'");
            var entry = await _ltm.CreateEntryAsync(category.Id, title, description, keywords, ct);

            // Build the fact
            var properties = new Dictionary<string, string?> { [key1] = value1 };
            if (!string.IsNullOrWhiteSpace(key2)) properties[key2] = value2;
            if (!string.IsNullOrWhiteSpace(key3)) properties[key3] = value3;
            if (!string.IsNullOrWhiteSpace(key4)) properties[key4] = value4;
            if (!string.IsNullOrWhiteSpace(key5)) properties[key5] = value5;

            var fact = new MemoryFact
            {
                FactType = fact_type,
                _properties = properties
            };

            await _ltm.AddFactToEntryAsync(entry.Id, fact, ct);

            return new ToolResult(true, $"Remembered. Memory ID: {entry.Id}");
        }

        // ------------------------------------------------------------------
        // 2. recall
        // ------------------------------------------------------------------
        [KernelFunction("recall")]
        [Description(
            "Search long-term memories by keywords. " +
            "Returns memories ranked by relevance (search match + freshness). " +
            "Memories that have decayed past their category's threshold are automatically deleted. " +
            "Use this to recall facts, preferences, or context about the user. " +
            "Searches across titles, descriptions, keywords, fact types, and fact property values. " +
            "Optionally filter by category name.")]
        public async Task<ToolResult> Recall(
            [Description("Keywords to search for. Provide 1-5 relevant terms.")] string[] keywords,
            [Description("Optional: category name to narrow the search. If omitted, searches all categories.")] string? category_name = null,
            [Description("Maximum results to return. Default 5.")] int max_results = 5,
            CancellationToken ct = default)
        {
            if (keywords == null || keywords.Length == 0)
                return new ToolResult(false, "At least one keyword is required.");

            await _chat.LogInfo($"Recalling memories for: {string.Join(", ", keywords)}");

            // Resolve category name to ID if provided
            string? categoryId = null;
            if (!string.IsNullOrWhiteSpace(category_name))
            {
                var category = await _ltm.GetCategoryByNameAsync(category_name, ct);
                if (category is null)
                    return new ToolResult(false, $"Category '{category_name}' not found.");
                categoryId = category.Id;
            }

            var results = await _ltm.SearchAsync(new MemorySearchOptions
            {
                Keywords = keywords,
                CategoryId = categoryId,
                MaxResults = max_results,
                MinRelevance = 0.1
            }, ct);

            var sb = new StringBuilder();

            // Report decay cleanup if any memories were purged
            // (SearchAsync auto-deletes decayed entries internally, but doesn't report count)
            // We run a separate decay purge to get the count for reporting
            int decayedDeleted = await _ltm.DeleteDecayedEntriesAsync(ct);
            if (decayedDeleted > 0)
            {
                sb.AppendLine($"[Auto-cleanup: {decayedDeleted} decayed memory(s) forgotten]");
                sb.AppendLine();
            }

            foreach (var r in results)
            {
                sb.AppendLine($"[ID: {r.Entry.Id}] {r.Entry.Title}");
                if (!string.IsNullOrEmpty(r.Entry.Description))
                    sb.AppendLine($"  Description: {r.Entry.Description}");
                if (r.Entry.Keywords?.Length > 0)
                    sb.AppendLine($"  Keywords: {string.Join(", ", r.Entry.Keywords)}");
                sb.AppendLine($"  Category: {r.Category.Name} | Relevance: {r.RelevanceScore:F2} | Decay: {r.DecayScore:F2}");

                if (r.Entry.Facts?.Count > 0)
                {
                    sb.AppendLine("  Facts:");
                    foreach (var fact in r.Entry.Facts)
                    {
                        sb.AppendLine($"    - [{fact.FactType}] {string.Join(", ", fact.Keys.Select(k => $"{k}={fact[k]}"))}");
                    }
                }

                sb.AppendLine();
            }

            var msg = results.Any()
                ? sb.ToString().TrimEnd()
                : "No memories matched your search.";

            return new ToolResult(true, msg);
        }

        // ------------------------------------------------------------------
        // 3. update_memory
        // ------------------------------------------------------------------
        [KernelFunction("update_memory")]
        [Description(
            "Update an existing memory's title, description, keywords, or facts. " +
            "Provide the memory ID and any fields you want to change. " +
            "To update a specific fact, provide the fact_id along with new fact_type and properties. " +
            "To add a new fact instead of replacing one, omit the fact_id.")]
        public async Task<ToolResult> UpdateMemory(
            [Description("The memory ID to update.")] string memory_id,
            [Description("Optional: new title for the memory.")] string? title = null,
            [Description("Optional: new description for the memory.")] string? description = null,
            [Description("Optional: new keywords to replace existing ones.")] string[]? keywords = null,
            [Description("Optional: the fact ID to update. If omitted, a new fact is added.")] string? fact_id = null,
            [Description("The fact type — a free-form label (e.g. 'preference', 'event', 'skill'). Required if adding/updating a fact.")] string? fact_type = null,
            [Description("First property key. Required if adding/updating a fact.")] string? key1 = null,
            [Description("First property value. Required if adding/updating a fact.")] string? value1 = null,
            [Description("Optional second property key.")] string? key2 = null,
            [Description("Optional second property value.")] string? value2 = null,
            [Description("Optional third property key.")] string? key3 = null,
            [Description("Optional third property value.")] string? value3 = null,
            [Description("Optional fourth property key.")] string? key4 = null,
            [Description("Optional fourth property value.")] string? value4 = null,
            [Description("Optional fifth property key.")] string? key5 = null,
            [Description("Optional fifth property value.")] string? value5 = null,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(memory_id))
                return new ToolResult(false, "memory_id is required.");

            var entry = await _ltm.GetEntryAsync(memory_id, ct);
            if (entry is null)
                return new ToolResult(false, $"Memory not found: {memory_id}");

            bool updated = false;

            // Update entry metadata
            if (!string.IsNullOrWhiteSpace(title))
            {
                entry.Title = title;
                updated = true;
            }

            if (description is not null) // Allow setting to empty string
            {
                entry.Description = description;
                updated = true;
            }

            if (keywords is not null)
            {
                entry.Keywords = keywords;
                updated = true;
            }

            // Update or add fact
            if (!string.IsNullOrWhiteSpace(fact_type) && !string.IsNullOrWhiteSpace(key1) && value1 is not null)
            {
                var properties = new Dictionary<string, string?> { [key1] = value1 };
                if (!string.IsNullOrWhiteSpace(key2)) properties[key2] = value2;
                if (!string.IsNullOrWhiteSpace(key3)) properties[key3] = value3;
                if (!string.IsNullOrWhiteSpace(key4)) properties[key4] = value4;
                if (!string.IsNullOrWhiteSpace(key5)) properties[key5] = value5;

                var updatedFact = new MemoryFact
                {
                    FactType = fact_type,
                    _properties = properties
                };

                if (!string.IsNullOrWhiteSpace(fact_id))
                {
                    // Update existing fact
                    await _ltm.UpdateFactAsync(memory_id, fact_id, updatedFact, ct);
                    await _chat.LogInfo($"Updated fact {fact_id} in memory: {entry.Title}");
                }
                else
                {
                    // Add new fact
                    await _ltm.AddFactToEntryAsync(memory_id, updatedFact, ct);
                    await _chat.LogInfo($"Added new fact to memory: {entry.Title}");
                }
                updated = true;
            }

            if (updated)
            {
                await _ltm.UpdateEntryAsync(entry, ct);
                return new ToolResult(true, "Memory updated.");
            }

            return new ToolResult(true, "No changes were made.");
        }

        // ------------------------------------------------------------------
        // 4. merge_memories
        // ------------------------------------------------------------------
        [KernelFunction("merge_memories")]
        [Description(
            "Merge two memories into one. The target memory keeps its ID; the source memory is deleted. " +
            "All facts from the source are moved to the target. Keywords are combined. " +
            "Use this to consolidate duplicate or related memories.")]
        public async Task<ToolResult> MergeMemories(
            [Description("The memory ID to keep (target).")] string target_memory_id,
            [Description("The memory ID to merge into the target and then delete (source).")] string source_memory_id,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(target_memory_id))
                return new ToolResult(false, "target_memory_id is required.");
            if (string.IsNullOrWhiteSpace(source_memory_id))
                return new ToolResult(false, "source_memory_id is required.");
            if (target_memory_id == source_memory_id)
                return new ToolResult(false, "Cannot merge a memory with itself.");

            var target = await _ltm.GetEntryAsync(target_memory_id, ct);
            if (target is null)
                return new ToolResult(false, $"Target memory not found: {target_memory_id}");

            var source = await _ltm.GetEntryAsync(source_memory_id, ct);
            if (source is null)
                return new ToolResult(false, $"Source memory not found: {source_memory_id}");

            var merged = await _ltm.MergeEntriesAsync(target_memory_id, source_memory_id, ct);
            await _chat.LogInfo($"Merged memory '{source.Title}' into '{target.Title}'");

            return new ToolResult(true, $"Merged successfully. Memory '{merged.Title}' now has {merged.Facts.Count} fact(s).");
        }

        // ------------------------------------------------------------------
        // 5. forget
        // ------------------------------------------------------------------
        [KernelFunction("forget")]
        [Description(
            "Delete a memory by its ID, or delete an entire category (and all its memories) by name. " +
            "Use with caution — deletion is permanent.")]
        public async Task<ToolResult> Forget(
            [Description("The memory ID to delete, OR a category name if 'is_category' is true.")] string id_or_name,
            [Description("Set to true if 'id_or_name' refers to a category name. All memories in that category will also be deleted. Default false.")] bool is_category = false,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id_or_name))
                return new ToolResult(false, "id_or_name is required.");

            if (is_category)
            {
                // Find category by name
                var cat = await _ltm.GetCategoryByNameAsync(id_or_name, ct);
                if (cat == null)
                    return new ToolResult(false, $"Category '{id_or_name}' not found.");

                await _chat.LogInfo($"Forgetting category '{cat.Name}' and all its memories...");
                await _ltm.DeleteCategoryAsync(cat.Id, ct);
                return new ToolResult(true, $"Category '{cat.Name}' and all associated memories have been forgotten.");
            }

            // Delete memory by ID
            var mem = await _ltm.GetEntryAsync(id_or_name, ct);
            if (mem == null)
                return new ToolResult(false, $"Memory not found: {id_or_name}");

            await _chat.LogInfo($"Forgetting memory: {mem.Title}");
            await _ltm.DeleteEntryAsync(id_or_name, ct);
            return new ToolResult(true, "Memory forgotten.");
        }

        // ------------------------------------------------------------------
        // 6. list_memory_categories
        // ------------------------------------------------------------------
        [KernelFunction("list_memory_categories")]
        [Description(
            "List all memory categories with their decay settings. " +
            "Use this to discover available categories before remembering or recalling memories.")]
        public async Task<ToolResult> ListMemoryCategories(CancellationToken ct = default)
        {
            var categories = await _ltm.GetCategoriesAsync(ct);
            if (!categories.Any())
                return new ToolResult(true, "No categories exist yet.");

            var sb = new StringBuilder();
            foreach (var c in categories)
            {
                sb.AppendLine($"[ID: {c.Id}] {c.Name}");
                if (!string.IsNullOrEmpty(c.Description))
                    sb.AppendLine($"  Description: {c.Description}");
                sb.AppendLine($"  Decay: {c.Decay}");
                sb.AppendLine();
            }

            return new ToolResult(true, sb.ToString().TrimEnd());
        }
    }
}
