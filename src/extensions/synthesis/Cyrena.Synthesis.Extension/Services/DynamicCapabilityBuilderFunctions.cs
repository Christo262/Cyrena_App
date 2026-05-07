using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Models;
using Cyrena.Synthesis.Contracts;
using Cyrena.Synthesis.Models;
using Cyrena.Synthesis.Options;
using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Cyrena.Synthesis.Services
{
    internal class DynamicCapabilityBuilderFunctions
    {
        private readonly ICapabilityStore _store;
        private readonly IChatMessageService _chat;
        private readonly IScriptEngine _engine;
        private readonly IScriptValidator _validator;
        private readonly SynthesisOptions _options;
        private readonly ICapabilityPermissionService _permissionService;
        private readonly SynthesisBuilder _builder;

        public DynamicCapabilityBuilderFunctions(ICapabilityStore store, IChatMessageService chat, IScriptEngine engine, IScriptValidator validator, SynthesisOptions options,
            ICapabilityPermissionService permissionService, SynthesisBuilder builder)
        {
            _store = store;
            _engine = engine;
            _validator = validator;
            _options = options;
            _chat = chat;
            _permissionService = permissionService;
            _builder = builder;
        }

        [KernelFunction("list_abi_descriptors")]
        [Description("Lists all registered ABI (Application Binary Interface) descriptors available to dynamic capabilities. " +
            "Returns the service type names of all exposed APIs. " +
            "Use 'get_abi_descriptor' to retrieve the full instruction for a specific ABI.")]
        public Task<string> ListAbiDescriptorsAsync(CancellationToken cancellationToken = default)
        {
            var descriptors = _builder.CapabilityAbis;
            if (descriptors.Count == 0)
            {
                return Task.FromResult("No ABI descriptors are currently registered.");
            }

            var sb = new StringBuilder();
            sb.AppendLine("Available ABI descriptors:");
            foreach (var descriptor in descriptors)
            {
                sb.AppendLine($"  - {descriptor.ServiceType.FullName}");
            }
            sb.AppendLine($"\nTotal: {descriptors.Count} ABI(s) registered.");
            sb.AppendLine("Use 'get_abi_descriptor' with the service type name to retrieve usage instructions.");

            return Task.FromResult(sb.ToString());
        }

        [KernelFunction("get_abi_descriptor")]
        [Description("Retrieves the full instruction documentation for a specific ABI descriptor by its service type name. " +
            "Provides detailed usage instructions for the exposed API, including available methods and examples. " +
            "Use 'list_abi_descriptors' first to discover available ABI names.")]
        public Task<string> GetAbiDescriptorAsync(
            [Description("The full service type name of the ABI descriptor (e.g., 'Cyrena.Synthesis.Contracts.IFileSystemAbi')")] string serviceTypeName,
            CancellationToken cancellationToken = default)
        {
            var descriptor = _builder.CapabilityAbis
                .FirstOrDefault(d => string.Equals(d.ServiceType.FullName, serviceTypeName, StringComparison.OrdinalIgnoreCase));

            if (descriptor == null)
            {
                var available = _builder.CapabilityAbis
                    .Select(d => d.ServiceType.FullName)
                    .ToList();

                var sb = new StringBuilder();
                sb.AppendLine($"ABI descriptor '{serviceTypeName}' not found.");
                if (available.Count > 0)
                {
                    sb.AppendLine("Available ABI descriptors:");
                    foreach (var name in available)
                    {
                        sb.AppendLine($"  - {name}");
                    }
                }
                else
                {
                    sb.AppendLine("No ABI descriptors are currently registered.");
                }
                return Task.FromResult(sb.ToString());
            }

            var result = new StringBuilder();
            result.AppendLine($"=== ABI: {descriptor.ServiceType.FullName} ===");
            result.AppendLine();
            result.AppendLine(descriptor.Instruction);
            result.AppendLine();
            result.AppendLine("=== End of ABI Instruction ===");

            return Task.FromResult(result.ToString());
        }

        [KernelFunction("search_abi_descriptors")]
        [Description("Searches registered ABI descriptors by keyword. " +
            "Returns matching ABI service type names and a preview of their instructions. " +
            "Useful for finding relevant APIs when you don't know the exact service type name.")]
        public Task<string> SearchAbiDescriptorsAsync(
            [Description("Keywords to search for in ABI instructions (space-separated)")] string keywords,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(keywords))
            {
                return Task.FromResult("Please provide search keywords.");
            }

            var searchTerms = keywords
                .Split([' ', ','], StringSplitOptions.RemoveEmptyEntries)
                .Select(k => k.Trim().ToLowerInvariant())
                .Where(k => k.Length > 0)
                .ToList();

            if (searchTerms.Count == 0)
            {
                return Task.FromResult("Please provide valid search keywords.");
            }

            var matches = _builder.CapabilityAbis
                .Where(d => searchTerms.Any(term =>
                    (!string.IsNullOrEmpty(d.ServiceType.FullName) && d.ServiceType.FullName.ToLowerInvariant().Contains(term)) ||
                    (!string.IsNullOrEmpty(d.Instruction) && d.Instruction.ToLowerInvariant().Contains(term))))
                .ToList();

            if (matches.Count == 0)
            {
                return Task.FromResult($"No ABI descriptors found matching keywords: '{keywords}'.");
            }

            var sb = new StringBuilder();
            sb.AppendLine($"Found {matches.Count} ABI descriptor(s) matching '{keywords}':");
            sb.AppendLine();

            foreach (var descriptor in matches)
            {
                sb.AppendLine($"--- {descriptor.ServiceType.FullName} ---");

                // Show a preview of the instruction (first 300 chars or first paragraph)
                var preview = GetInstructionPreview(descriptor.Instruction, 300);
                sb.AppendLine(preview);
                sb.AppendLine();
            }
            sb.AppendLine("Use 'get_abi_descriptor' with the full service type name to retrieve the complete instruction.");

            return Task.FromResult(sb.ToString());
        }
        [KernelFunction("create")]
        [Description("Creates a new dynamic capability with F# code. " +
            "The AI must provide a function-like ID for the capability (e.g., 'writeToJournal', 'calculateFibonacci'). " +
            "This ID will be used to execute the capability and must be unique. " +
            "The capability will be validated for restricted patterns before saving. " +
            "Returns the created capability details including its assigned ID.")]
        public async Task<string> CreateDynamicCapabilityAsync(
            [Description("A unique function-like identifier for the capability in camelCase (e.g., 'writeToJournal', 'calculateFibonacci'). This ID is used to execute the capability.")] string id,
            [Description("A short, descriptive title for the capability (e.g., 'Calculate Fibonacci')")] string title,
            [Description("A detailed description of what the capability does and when to use it")] string description,
            [Description("The F# code for the capability. Must contain a 'main' function accepting ICapabilityExecutionContext.")] string code,
            [Description("Optional keywords for discoverability (comma-separated, e.g., 'math, calculation, fibonacci')")] string? keywords = null,
            [Description("Optional argument definitions as key-value pairs in format 'name:type:description' (e.g., 'n:int:The number to calculate'). Multiple arguments separated by semicolons.")] string? arguments = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(id))
                return "Error: ID is required. Provide a function-like identifier in camelCase (e.g., 'writeToJournal').";
            if (string.IsNullOrWhiteSpace(title))
                return "Error: Title is required.";
            if (string.IsNullOrWhiteSpace(code))
                return "Error: Code is required.";

            // Normalize the ID to camelCase function-like format
            var normalizedId = ToCamelCaseId(id);

            // Check if a capability with this ID already exists
            var existingById = await _store.GetByIdAsync(normalizedId, cancellationToken);
            if (existingById != null)
            {
                return $"Error: A dynamic capability with ID '{normalizedId}' already exists (Title: {existingById.Title}). Use a different ID or update the existing capability.";
            }

            // Check if a capability with this title already exists
            var existing = await _store.GetByTitleAsync(title, cancellationToken);
            if (existing != null)
            {
                return $"Error: A dynamic capability with title '{title}' already exists (ID: {existing.Id}). Use a different title or update the existing capability.";
            }

            // Validate the code if validation is enabled
            if (_options.ValidateScriptsBeforeExecution)
            {
                var validation = await _validator.ValidateAsync(code, cancellationToken);
                if (!validation.IsValid && _options.RejectInvalidScripts)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("Error: Code validation failed. The capability was not created.");
                    sb.AppendLine($"Summary: {validation.Summary}");
                    sb.AppendLine("Violations:");
                    foreach (var violation in validation.Violations)
                    {
                        sb.AppendLine($"  - [{violation.Type}] Line {violation.LineNumber}: {violation.Reason}");
                        sb.AppendLine($"    Pattern: {violation.Pattern}");
                    }
                    return sb.ToString();
                }
                if (!validation.IsValid)
                {
                    await _chat.LogWarn($"Creating capability '{title}' with {validation.Violations.Count} validation warnings.");
                }
            }

            // Parse keywords
            var keywordList = new List<string>();
            if (!string.IsNullOrWhiteSpace(keywords))
            {
                keywordList = keywords.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(k => k.Trim())
                    .Where(k => k.Length > 0)
                    .ToList();
            }

            // Parse arguments
            var argumentList = new List<ScriptArgument>();
            if (!string.IsNullOrWhiteSpace(arguments))
            {
                var argDefs = arguments.Split(';', StringSplitOptions.RemoveEmptyEntries);
                foreach (var argDef in argDefs)
                {
                    var parts = argDef.Split(':', 3);
                    if (parts.Length >= 2)
                    {
                        var argType = parts[1].Trim().ToLowerInvariant();
                        // Normalize type names
                        argType = argType switch
                        {
                            "integer" => "int",
                            "boolean" => "bool",
                            "float" => "double",
                            "number" => "double",
                            _ => argType
                        };

                        argumentList.Add(new ScriptArgument
                        {
                            Name = parts[0].Trim(),
                            Type = argType,
                            Description = parts.Length > 2 ? parts[2].Trim() : string.Empty,
                            IsRequired = true
                        });
                    }
                }
            }

            var capability = new DynamicCapability
            {
                Id = normalizedId,
                Title = title.Trim(),
                Description = description?.Trim() ?? string.Empty,
                Code = code.Trim(),
                Keywords = keywordList,
                Arguments = argumentList,
                IsEnabled = true,
                CreatedAt = DateTime.UtcNow,
                ModifiedAt = DateTime.UtcNow,
                Version = 1
            };

            var saved = await _store.SaveAsync(capability, cancellationToken);

            var result = new StringBuilder();
            result.AppendLine($"Dynamic capability created successfully.");
            result.AppendLine($"  ID: {saved.Id}");
            result.AppendLine($"  Title: {saved.Title}");
            result.AppendLine($"  Version: {saved.Version}");
            result.AppendLine($"  Arguments: {saved.Arguments.Count}");
            if (saved.Arguments.Count > 0)
            {
                foreach (var arg in saved.Arguments)
                {
                    result.AppendLine($"    - {arg.Name} ({arg.Type}): {arg.Description}");
                }
            }
            result.AppendLine($"  Keywords: {(saved.Keywords.Count > 0 ? string.Join(", ", saved.Keywords) : "(none)")}");

            await _chat.LogInfo($"Created dynamic capability '{saved.Title}' (ID: {saved.Id}).");
            return result.ToString();
        }

        [KernelFunction("delete")]
        [Description("Deletes a dynamic capability by its ID. " +
            "This permanently removes the capability and cannot be undone. " +
            "Returns confirmation of deletion or an error if the capability was not found.")]
        public async Task<string> DeleteDynamicCapabilityAsync(
            [Description("The ID of the dynamic capability to delete")] string capabilityId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(capabilityId))
                return "Error: Capability ID is required.";

            var capability = await _store.GetByIdAsync(capabilityId, cancellationToken);
            if (capability == null)
            {
                return $"Error: Dynamic capability with ID '{capabilityId}' not found.";
            }

            var deleted = await _store.DeleteAsync(capabilityId, cancellationToken);
            if (!deleted)
            {
                return $"Error: Failed to delete dynamic capability '{capability.Title}' (ID: {capabilityId}).";
            }
            // Clean up any permissions associated with this capability
            var deletedCount = await _permissionService.DeleteAllPermissionsAsync(capabilityId, cancellationToken);

            await _chat.LogInfo($"Deleted dynamic capability '{capability.Title}' (ID: {capabilityId})." +
                (deletedCount > 0 ? $" Deleted {deletedCount} associated permission(s)." : ""));
            return $"Dynamic capability '{capability.Title}' (ID: {capabilityId}) has been permanently deleted." +
                (deletedCount > 0 ? $" {deletedCount} associated permission(s) were also deleted." : "");
        }

        [KernelFunction("view")] // => Builder_view
        [Description(
            "Retrieves the full details of a Dynamic Capability by its ID, including metadata, arguments, description, and F# source code. " +
            "Useful for inspecting, debugging, validating, updating, or reviewing existing capabilities before execution or deletion.")]
        public async Task<ToolResult<DynamicCapability>> ViewAsync(
            [Description("The ID of the dynamic capability to view")] string capabilityId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(capabilityId))
                return new ToolResult<DynamicCapability>(false, "capabilityId cannot be empty");
            var capability = await _store.GetByIdAsync(capabilityId, cancellationToken);
            if (capability == null)
                return new ToolResult<DynamicCapability>(false, "Capability not found");
            return new ToolResult<DynamicCapability>(capability);
        }

        [KernelFunction("validate")]
        [Description("Validates and compiles a Dynamic Capability")]
        public async Task<CapabilityExecutionResult> ValidateAsync(
            [Description("The ID of the dynamic capability to validate")] string capabilityId, CancellationToken cancellationToken = default)
        {
            var capability = await _store.GetByIdAsync(capabilityId, cancellationToken);
            if (capability == null)
                return new CapabilityExecutionResult()
                {
                    Success = false,
                    Error = $"Capability {capabilityId} not found"
                };
            var result = await _engine.ValidateAsync(capability.Code, cancellationToken);
            return result;
        }


        /// <summary>
        /// Extracts a preview from an instruction string, preferring the first paragraph
        /// and truncating with ellipsis if necessary.
        /// </summary>
        private static string GetInstructionPreview(string instruction, int maxLength)
        {
            if (string.IsNullOrEmpty(instruction))
                return "(No instruction available)";

            // Try to get first paragraph
            var firstParagraphEnd = instruction.IndexOf("\n\n", StringComparison.Ordinal);
            var preview = firstParagraphEnd > 0
                ? instruction[..firstParagraphEnd].Trim()
                : instruction.Trim();

            if (preview.Length > maxLength)
            {
                preview = preview[..maxLength].TrimEnd() + "...";
            }

            return preview;
        }

        /// <summary>
        /// Converts a user-provided string into a camelCase function-like identifier.
        /// Removes non-alphanumeric characters, splits on spaces/separators, and lowercases
        /// the first word while uppercasing the first letter of subsequent words.
        /// Examples: "write to journal" → "writeToJournal", "Calculate-Fibonacci" → "calculateFibonacci".
        /// </summary>
        private static string ToCamelCaseId(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            var normalized = new StringBuilder();

            foreach (var c in input)
            {
                if (char.IsLetterOrDigit(c))
                {
                    normalized.Append(c);
                }
                else if (c == ' ' || c == '-' || c == '_' || c == '.')
                {
                    normalized.Append(' ');
                }
            }

            var words = normalized.ToString()
                .Split([' '], StringSplitOptions.RemoveEmptyEntries);

            if (words.Length == 0)
                return string.Empty;

            var result = new StringBuilder();

            // First word
            result.Append(char.ToLowerInvariant(words[0][0]));
            if (words[0].Length > 1)
                result.Append(words[0][1..]);

            // Remaining words
            for (int i = 1; i < words.Length; i++)
            {
                result.Append(char.ToUpperInvariant(words[i][0]));

                if (words[i].Length > 1)
                    result.Append(words[i][1..]);
            }

            return result.ToString();
        }
    }
}
