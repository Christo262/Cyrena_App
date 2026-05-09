namespace Cyrena.Synthesis.Options
{
    /// <summary>
    /// Configuration options for the Cyrena.FX controlled capability F# dynamic capability extension.
    ///
    /// Security model:
    /// - Dynamic capabilities operate through Cyrena APIs only (ICyrenaScriptContext)
    /// - Capabilities are feature-gated via permissions
    /// - User remains in control with explicit approval model
    /// - Dynamic capabilities are isolated and permission-scoped
    /// - AssemblyLoadContext is NOT considered a security boundary
    /// - Planned: worker process isolation for true sandboxing
    /// </summary>
    public class SynthesisOptions
    {
        /// <summary>
        /// The settings key used to store FX configuration.
        /// </summary>
        public const string Key = "cyrena.synthesis";

        public const string WorkingDirectoryKey = "synthesis.working.directory";
        public const string AssistantId = "cyrena.synthesis.builder";

        /// <summary>
        /// Whether the F# dynamic capability runtime is enabled.
        /// When disabled, dynamic capability execution does not exist and
        /// model/tooling cannot see or use dynamic capability APIs.
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// The root directory for sandboxed file system operations.
        /// All file operations from dynamic capabilities are restricted to this directory.
        /// Defaults to a Documents/Cyréna/Sandbox
        /// </summary>
        public string SandboxRootDirectory { get; set; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "Cyréna");

        /// <summary>
        /// Maximum execution time for a dynamic capability before it is cancelled.
        /// </summary>
        public TimeSpan MaxExecutionTime { get; set; } = TimeSpan.FromMinutes(1);

        /// <summary>
        /// Whether to enable verbose logging of dynamic capability execution.
        /// </summary>
        public bool VerboseLogging { get; set; } = false;

        /// <summary>
        /// Whether dynamic capabilities require explicit permission grants before execution.
        /// When true, each dynamic capability must be granted permission before it can run.
        /// When false, dynamic capabilities run with default permissions.
        ///
        /// Recommended: true for production, false for development.
        /// </summary>
        public bool RequireExplicitPermission { get; set; } = true;

        /// <summary>
        /// Whether to run dynamic capability validation before compilation.
        /// Validates that dynamic capabilities do not contain restricted patterns.
        /// This is a guardrail, not a security boundary.
        /// </summary>
        public bool ValidateScriptsBeforeExecution { get; set; } = true;

        /// <summary>
        /// Whether to reject dynamic capabilities that fail validation.
        /// When true, dynamic capabilities with violations cannot be executed.
        /// When false, violations are logged but execution proceeds.
        /// </summary>
        public bool RejectInvalidScripts { get; set; } = true;

        /// <summary>
        /// Whether to persist dynamic capability execution history.
        /// </summary>
        public bool PersistExecutionHistory { get; set; } = true;

        /// <summary>
        /// List of approved assembly references for F# dynamic capabilities.
        /// Only these assemblies are exposed to the dynamic capability compilation
        /// via -r: flags (resolved by filename inside the framework directory using
        /// --simpleresolution and --lib:).
        ///
        /// IMPORTANT: System.Private.CoreLib.dll must NOT appear here.
        /// The F# compiler (when using --noframework + --simpleresolution) treats
        /// System.Runtime.dll as the authoritative type facade and resolves its
        /// type-forwards into Private.CoreLib internally. Passing Private.CoreLib
        /// as an explicit -r: alongside System.Runtime causes the compiler to see
        /// two definitions of every primitive type, producing errors like:
        ///   "The type 'X' is required here and is unavailable. You must add a
        ///    reference to assembly 'System.Runtime'."
        /// even though System.Runtime is already referenced.
        ///
        /// Restricted assemblies (NOT included):
        /// - System.Private.CoreLib.dll (implementation assembly; must not be -r:)
        /// - System.IO.dll (dynamic capabilities use ICyrenaFileApi)
        /// - System.Net.Http.dll (network access restricted)
        /// - System.Diagnostics.Process (process execution restricted)
        /// - Reflection-heavy assemblies
        ///
        /// Approved references:
        /// - System.Runtime.dll (core BCL facade; authoritative for type resolution)
        /// - System.Collections.dll (collections)
        /// - System.Linq.dll (LINQ)
        /// - System.Threading.dll (threading primitives)
        /// - System.Threading.Tasks.dll (async/await)
        /// - FSharp.Core.dll (F# language support; added separately with full path)
        /// - Cyrena.FX.Contracts.dll (Cyrena ABI; added separately with full path)
        /// </summary>
        public List<string> ApprovedReferences { get; set; } = new()
        {
            // Core runtime facade.
            // Do NOT add System.Private.CoreLib.dll — see XML doc above.
            "System.Runtime.dll",

            // Collections / LINQ
            "System.Collections.dll",
            "System.Collections.Immutable.dll",
            "System.Collections.Concurrent.dll",
            "System.Collections.NonGeneric.dll",
            "System.Collections.Specialized.dll",
            "netstandard.dll",
            "System.Linq.dll",

            // Threading / Tasks
            "System.Threading.dll",
            "System.Threading.Tasks.dll",

            // Common utility assemblies
            "System.Text.RegularExpressions.dll",
            "System.Numerics.dll",
            "System.Globalization.dll",

            // Optional but useful
            "System.Console.dll",

            // F# Core is added separately in CompileScriptAsync via full path
            // because it lives in the app directory, not the framework directory.
            "FSharp.Core.dll"
        };

        /// <summary>
        /// List of explicitly blocked assembly references.
        /// These assemblies are never exposed to dynamic capabilities even if requested.
        /// </summary>
        public List<string> BlockedReferences { get; set; } = new()
        {
            // Raw filesystem access
            "System.IO.dll",
            "System.IO.FileSystem.dll",
            "System.IO.FileSystem.Primitives.dll",

            // Networking
            "System.Net.Http.dll",
            "System.Net.Requests.dll",
            "System.Net.Primitives.dll",
            "System.Net.Sockets.dll",

            // Process execution / diagnostics
            "System.Diagnostics.Process.dll",
            "System.Diagnostics.Debug.dll",
            "System.Diagnostics.TraceSource.dll",

            // Reflection / dynamic runtime manipulation
            "System.Reflection.dll",
            "System.Reflection.Emit.dll",
            "System.Reflection.Emit.ILGeneration.dll",
            "System.Reflection.Emit.Lightweight.dll",
            "System.Reflection.Metadata.dll",
            "System.Reflection.Primitives.dll",
            "System.Reflection.TypeExtensions.dll",

            // Native interop
            "System.Runtime.InteropServices.dll",
            "System.Runtime.InteropServices.RuntimeInformation.dll",

            // Security-sensitive assemblies
            "System.Security.AccessControl.dll",
            "System.Security.Claims.dll",
            "System.Security.Cryptography.dll",
            "System.Security.Principal.dll",
            "System.Security.Principal.Windows.dll",

            // Windows APIs
            "Microsoft.Win32.Primitives.dll",
            "Microsoft.Win32.Registry.dll"
        };

        /// <summary>
        /// Whether to use the worker process isolation architecture.
        /// When enabled, dynamic capabilities execute in a separate process with restricted permissions.
        /// When disabled, dynamic capabilities execute in-process (less secure, for development only).
        ///
        /// NOTE: Worker process isolation is the planned secure architecture.
        /// Current implementation uses in-process execution with capability gating.
        /// </summary>
        public bool UseWorkerProcessIsolation { get; set; } = false;

        /// <summary>
        /// The timeout for worker process communication.
        /// </summary>
        public TimeSpan WorkerProcessTimeout { get; set; } = TimeSpan.FromMinutes(5);

        /// <summary>
        /// The maximum memory allowed for a dynamic capability execution (in MB).
        /// </summary>
        public int MaxMemoryMB { get; set; } = 256;
    }
}