using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Synthesis.Contracts;
using Cyrena.Synthesis.Models;
using Cyrena.Synthesis.Options;
using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Text;

namespace Cyrena.Synthesis.Services
{
    internal class DynamicCapabilityConsumerFunctions
    {
        private readonly ICapabilityStore _store;
        private readonly IChatMessageService _chat;
        private readonly IScriptEngine _engine;
        private readonly IScriptValidator _validator;
        private readonly SynthesisOptions _options;
        private readonly ICapabilityPermissionService _permissionService;
        private readonly SynthesisBuilder _builder;

        public DynamicCapabilityConsumerFunctions(ICapabilityStore store, IChatMessageService chat, IScriptEngine engine, IScriptValidator validator, SynthesisOptions options,
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

        [KernelFunction("execute")]
        [Description("Executes a dynamic capability by its ID with structured typed arguments. " +
            "Requires 'Script.Execute' permission. " +
            "User approval is required before execution. " +
            "Dynamic capabilities access arguments by name: ctx.Args.GetString(\"name\"), ctx.Args.GetInt32(\"name\"), etc. " +
            "Never use raw positional indexing like args.[0]. " +
            "Returns the execution result.")]
        public async Task<CapabilityExecutionResult> ExecuteDynamicCapabilityAsync(
            [Description("The ID of the dynamic capability to execute")] string capabilityId,
            [Description("Structured typed arguments for the dynamic capability. Each argument has Name, Type, and Value.")] ScriptArgument[]? arguments = null,
            [Description("Optional timeout override for this execution. Use 0 for default timeout.")] int timeoutSeconds = 0,
            CancellationToken cancellationToken = default)
        {
            var capability = await GetCapabilityAsync(capabilityId, cancellationToken);
            if (capability is CapabilityExecutionResult errorResult)
                return errorResult;

            // Build the structured execution request
            var request = new CapabilityRequest
            {
                ScriptId = capabilityId,
                Arguments = arguments?.ToList() ?? new(),
                Timeout = timeoutSeconds == 0 ? null : TimeSpan.FromSeconds(timeoutSeconds),
                ValidateBeforeExecution = true
            };

            await _chat.LogInfo($"Executing dynamic capability '{((DynamicCapability)capability).Title}' with {request.Arguments.Count} argument(s)...");
            var result = await _engine.ExecuteAsync((DynamicCapability)capability, request, cancellationToken);

            await LogResultAsync(result);
            return result;
        }

        [KernelFunction("execute_simple")]
        [Description("Executes a dynamic capability by its ID with simple string arguments. " +
            "Arguments are passed as key-value pairs. " +
            "Requires 'Script.Execute' permission. " +
            "Dynamic capabilities access arguments by name: ctx.Args.GetString(\"name\"). " +
            "Returns the execution result.")]
        public async Task<CapabilityExecutionResult> ExecuteDynamicCapabilitySimpleAsync(
            [Description("The ID of the dynamic capability to execute")] string capabilityId,
            [Description("Simple string arguments as key-value pairs (e.g., filePath=example.txt, count=5)")] string? args = null,
            CancellationToken cancellationToken = default)
        {
            var capability = await GetCapabilityAsync(capabilityId, cancellationToken);
            if (capability is CapabilityExecutionResult errorResult)
                return errorResult;

            // Parse simple string arguments into ScriptArgument list
            var arguments = new List<ScriptArgument>();
            if (!string.IsNullOrEmpty(args))
            {
                var pairs = args.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var pair in pairs)
                {
                    var kv = pair.Split('=', 2);
                    if (kv.Length == 2)
                    {
                        arguments.Add(new ScriptArgument
                        {
                            Name = kv[0].Trim(),
                            Type = "string",
                            Value = kv[1].Trim()
                        });
                    }
                }
            }

            var request = new CapabilityRequest
            {
                ScriptId = capabilityId,
                Arguments = arguments,
                ValidateBeforeExecution = true
            };

            await _chat.LogInfo($"Executing dynamic capability '{((DynamicCapability)capability).Title}' with {request.Arguments.Count} argument(s)...");
            var result = await _engine.ExecuteAsync((DynamicCapability)capability, request, cancellationToken);

            await LogResultAsync(result);
            return result;
        }

        

        [KernelFunction("list")]
        [Description("Lists all dynamic capabilities with their metadata. " +
            "Returns a formatted list including ID, title, description, keywords, and enabled status. " +
            "Use 'search' to find capabilities by keywords or title.")]
        public async Task<string> ListDynamicCapabilitiesAsync(CancellationToken cancellationToken = default)
        {
            var capabilities = await _store.GetAllAsync(cancellationToken);

            if (capabilities.Count == 0)
            {
                return "No dynamic capabilities found. Use 'create' to add a new capability.";
            }

            var sb = new StringBuilder();
            sb.AppendLine($"Found {capabilities.Count} dynamic capability(s):");
            sb.AppendLine();

            foreach (var cap in capabilities)
            {
                sb.AppendLine($"--- {cap.Title} ---");
                sb.AppendLine($"  ID: {cap.Id}");
                sb.AppendLine($"  Description: {(string.IsNullOrWhiteSpace(cap.Description) ? "(none)" : cap.Description)}");
                sb.AppendLine($"  Enabled: {cap.IsEnabled}");
                sb.AppendLine($"  Version: {cap.Version}");
                sb.AppendLine($"  Arguments: {cap.Arguments.Count}");
                if (cap.Arguments.Count > 0)
                {
                    foreach (var arg in cap.Arguments)
                    {
                        sb.AppendLine($"    - {arg.Name} ({arg.Type}){(string.IsNullOrWhiteSpace(arg.Description) ? "" : $": {arg.Description}")}");
                    }
                }
                sb.AppendLine($"  Keywords: {(cap.Keywords.Count > 0 ? string.Join(", ", cap.Keywords) : "(none)")}");
                sb.AppendLine($"  Modified: {cap.ModifiedAt:yyyy-MM-dd HH:mm:ss} UTC");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        [KernelFunction("search")]
        [Description("Searches dynamic capabilities by keywords or title. " +
            "Returns matching capabilities with their metadata. " +
            "Use 'list' to see all capabilities.")]
        public async Task<string> SearchDynamicCapabilitiesAsync(
            [Description("Search keywords to match against capability titles and keywords (space-separated)")] string keywords,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(keywords))
                return "Error: Search keywords are required.";

            var searchTerms = keywords
                .Split([' ', ','], StringSplitOptions.RemoveEmptyEntries)
                .Select(k => k.Trim().ToLowerInvariant())
                .Where(k => k.Length > 0)
                .ToList();

            if (searchTerms.Count == 0)
                return "Error: Please provide valid search keywords.";

            // Search by keywords first
            var byKeywords = await _store.SearchByKeywordsAsync(searchTerms, cancellationToken);

            // Search by title
            var byTitle = await _store.SearchByTitleAsync(keywords, cancellationToken);

            // Combine and deduplicate by ID
            var allMatches = byKeywords.Concat(byTitle)
                .GroupBy(c => c.Id)
                .Select(g => g.First())
                .ToList();

            if (allMatches.Count == 0)
            {
                return $"No dynamic capabilities found matching '{keywords}'.";
            }

            var sb = new StringBuilder();
            sb.AppendLine($"Found {allMatches.Count} dynamic capability(s) matching '{keywords}':");
            sb.AppendLine();

            foreach (var cap in allMatches)
            {
                sb.AppendLine($"--- {cap.Title} ---");
                sb.AppendLine($"  ID: {cap.Id}");
                sb.AppendLine($"  Description: {(string.IsNullOrWhiteSpace(cap.Description) ? "(none)" : cap.Description)}");
                sb.AppendLine($"  Enabled: {cap.IsEnabled}");
                sb.AppendLine($"  Version: {cap.Version}");
                sb.AppendLine($"  Keywords: {(cap.Keywords.Count > 0 ? string.Join(", ", cap.Keywords) : "(none)")}");
                sb.AppendLine();
            }

            return sb.ToString();
        }


        

        /// <summary>
        /// Shared helper: logs execution result output and errors.
        /// </summary>
        private async Task LogResultAsync(CapabilityExecutionResult result)
        {
            if (!string.IsNullOrEmpty(result.Output))
                await _chat.LogInfo(result.Output);
            if (!string.IsNullOrEmpty(result.Error))
                await _chat.LogError(result.Error);
        }

        private async Task<object> GetCapabilityAsync(string capabilityId, CancellationToken cancellationToken)
        {
            var capability = await _store.GetByIdAsync(capabilityId, cancellationToken);
            if (capability == null)
            {
                return new CapabilityExecutionResult
                {
                    Success = false,
                    Error = $"Dynamic capability with ID '{capabilityId}' not found.",
                    ScriptId = capabilityId
                };
            }

            if (!capability.IsEnabled)
            {
                return new CapabilityExecutionResult
                {
                    Success = false,
                    Error = $"Dynamic capability '{capability.Title}' is disabled.",
                    ScriptId = capabilityId
                };
            }

            return capability;
        }
    }
}
