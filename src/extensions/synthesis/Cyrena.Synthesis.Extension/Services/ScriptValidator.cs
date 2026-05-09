using Cyrena.Synthesis.Contracts;
using Cyrena.Synthesis.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Cyrena.Synthesis.Services
{
    /// <summary>
    /// Validates F# dynamic capability code for restricted patterns before compilation.
    ///
    /// IMPORTANT: This is a guardrail layer, NOT a security boundary.
    /// The true security boundary is the planned worker process isolation architecture:
    /// 1. Compile dynamic capability
    /// 2. Execute in isolated worker process
    /// 3. Pass approved permissions/context into worker
    /// 4. Communicate via JSON IPC or structured messages
    /// 5. Enforce execution timeout
    /// 6. Restrict filesystem access to approved directories
    /// 7. Destroy worker after execution
    ///
    /// AssemblyLoadContext is NOT considered a security boundary.
    /// </summary>
    internal class ScriptValidator : IScriptValidator
    {
        /// <summary>
        /// Restricted namespace patterns that dynamic capabilities must not open or reference.
        /// These provide direct system access that bypasses the Cyrena ABI.
        /// </summary>
        private static readonly IReadOnlyList<RestrictedPattern> RestrictedNamespaces = new List<RestrictedPattern>
        {
            new("open System.IO", "Dynamic capabilities must use ctx.Files API instead of System.IO directly."),
            new("open System.Net", "Network access is restricted. Use Cyrena-managed APIs only."),
            new("open System.Net.Http", "HTTP access is restricted. Use Cyrena-managed APIs only."),
            new("open System.Diagnostics", "Process execution is restricted. Use Cyrena-managed APIs only."),
            new("open System.Reflection", "Reflection is restricted. Dynamic capabilities must operate through Cyrena APIs only."),
            new("open System.Runtime.InteropServices", "Interop is restricted for security."),
            new("open System.Security", "Security namespace access is restricted."),
            new("open System.Threading.ThreadPool", "Thread pool manipulation is restricted."),
            new("open Microsoft.Win32", "Registry access is restricted."),
        };

        /// <summary>
        /// Restricted type references that indicate direct system access.
        /// </summary>
        private static readonly IReadOnlyList<RestrictedPattern> RestrictedTypes = new List<RestrictedPattern>
        {
            new("System.IO.File", "Use ctx.Files.ReadText/WriteText instead."),
            new("System.IO.Directory", "Use ctx.Files.ListDirectories/CreateDirectory instead."),
            new("System.IO.Path", "Path manipulation is restricted. Use relative paths with ctx.Files API."),
            new("System.IO.FileStream", "Direct file stream access is restricted. Use ctx.Files API."),
            new("System.IO.StreamReader", "Direct stream access is restricted. Use ctx.Files API."),
            new("System.IO.StreamWriter", "Direct stream access is restricted. Use ctx.Files API."),
            new("System.Net.Http.HttpClient", "HTTP client access is restricted. Use Cyrena-managed APIs."),
            new("System.Net.WebClient", "Web client access is restricted. Use Cyrena-managed APIs."),
            new("System.Net.WebRequest", "Web request access is restricted. Use Cyrena-managed APIs."),
            new("System.Diagnostics.Process", "Process execution is restricted. Use Cyrena-managed APIs."),
            new("System.Diagnostics.ProcessStartInfo", "Process execution is restricted. Use Cyrena-managed APIs."),
            new("System.Reflection.Assembly", "Reflection assembly loading is restricted."),
            new("System.Reflection.TypeInfo", "Reflection type inspection is restricted."),
            new("System.Reflection.MethodInfo", "Reflection method inspection is restricted."),
            new("System.Activator", "Dynamic object creation is restricted."),
            new("System.AppDomain", "AppDomain manipulation is restricted."),
            new("System.Environment", "Environment access is restricted. Use ctx.Log for output."),
            new("System.Console", "Console access is restricted. Use ctx.Log for output."),
        };

        /// <summary>
        /// Restricted method calls that indicate dangerous operations.
        /// </summary>
        private static readonly IReadOnlyList<RestrictedPattern> RestrictedMethods = new List<RestrictedPattern>
        {
            new("File.ReadAllText", "Use ctx.Files.ReadText instead."),
            new("File.ReadAllLines", "Use ctx.Files.ReadText instead."),
            new("File.WriteAllText", "Use ctx.Files.WriteText instead."),
            new("File.WriteAllLines", "Use ctx.Files.WriteText instead."),
            new("File.Delete", "Use ctx.Files.Delete instead."),
            new("File.Exists", "Use ctx.Files.Exists instead."),
            new("File.Copy", "File copy is restricted. Use ctx.Files API."),
            new("File.Move", "File move is restricted. Use ctx.Files API."),
            new("Directory.GetFiles", "Use ctx.Files.ListFiles instead."),
            new("Directory.GetDirectories", "Use ctx.Files.ListDirectories instead."),
            new("Directory.CreateDirectory", "Use ctx.Files.CreateDirectory instead."),
            new("Directory.Delete", "Use ctx.Files.DeleteDirectory instead."),
            new("Directory.Exists", "Use ctx.Files.Exists instead."),
            new("Process.Start", "Process execution is restricted. Use Cyrena-managed APIs."),
            new("Activator.CreateInstance", "Dynamic object creation is restricted."),
            new("Assembly.Load", "Dynamic assembly loading is restricted."),
            new("Assembly.LoadFrom", "Dynamic assembly loading is restricted."),
            new("Assembly.LoadFile", "Dynamic assembly loading is restricted."),
            new("Type.GetType", "Reflection type loading is restricted."),
            new("GetType(", "Reflection type inspection is restricted."),
        };

        /// <summary>
        /// Restricted compiler directives that could bypass reference restrictions.
        /// </summary>
        private static readonly IReadOnlyList<RestrictedPattern> RestrictedDirectives = new List<RestrictedPattern>
        {
            new("#r ", "External reference directives are restricted. Only approved Cyrena ABI references are allowed."),
            new("#load ", "External dynamic capability loading is restricted."),
            new("#I ", "Assembly search path modification is restricted."),
        };

        /// <summary>
        /// Restricted keywords that indicate unsafe operations.
        /// </summary>
        private static readonly IReadOnlyList<RestrictedPattern> RestrictedKeywords = new List<RestrictedPattern>
        {
            new("extern", "External function declarations are restricted."),
            new("DllImport", "P/Invoke is restricted for security."),
            new("Marshal", "Marshalling operations are restricted."),
            new("unsafe", "Unsafe code blocks are restricted."),
            new("nativeint", "Native pointer types are restricted."),
            new("unativeint", "Native pointer types are restricted."),
        };

        /// <summary>
        /// Patterns that indicate raw positional argument access.
        /// Dynamic capabilities must use ctx.Args.GetString("name") instead of args.[0].
        /// </summary>
        private static readonly IReadOnlyList<RestrictedPattern> RestrictedArgumentPatterns = new List<RestrictedPattern>
        {
            new("args.[", "Raw positional argument access is prohibited. Use ctx.Args.GetString(\"name\") instead."),
            new("string[] args", "Raw string[] args parameter is deprecated. Use ICyrenaScriptContext parameter instead."),
            new("args: string[]", "Raw string[] args parameter is deprecated. Use ICyrenaScriptContext parameter instead."),
            new("Array.get args", "Raw positional argument access is prohibited. Use ctx.Args.GetString(\"name\") instead."),
        };

        /// <summary>
        /// Patterns that indicate unnecessary async complexity in dynamic capabilities.
        /// Dynamic capabilities should use synchronous Cyrena ABI methods instead.
        /// </summary>
        private static readonly IReadOnlyList<RestrictedPattern> RestrictedAsyncPatterns = new List<RestrictedPattern>
        {
            new("Async.AwaitTask", "Dynamic capabilities must use synchronous Cyrena ABI methods. Do not use Async.AwaitTask. Use ctx.Files.ReadText/WriteText instead of ReadTextAsync/WriteTextAsync."),
            new("Async.RunSynchronously", "Dynamic capabilities must use synchronous Cyrena ABI methods. Do not use Async.RunSynchronously. Use ctx.Files.ReadText/WriteText instead of ReadTextAsync/WriteTextAsync."),
            new("ReadTextAsync", "Dynamic capabilities must use synchronous Cyrena ABI methods. Use ctx.Files.ReadText instead of ReadTextAsync."),
            new("WriteTextAsync", "Dynamic capabilities must use synchronous Cyrena ABI methods. Use ctx.Files.WriteText instead of WriteTextAsync."),
            new("ExistsAsync", "Dynamic capabilities must use synchronous Cyrena ABI methods. Use ctx.Files.Exists instead of ExistsAsync."),
            new("DeleteAsync", "Dynamic capabilities must use synchronous Cyrena ABI methods. Use ctx.Files.Delete instead of DeleteAsync."),
            new("ListFilesAsync", "Dynamic capabilities must use synchronous Cyrena ABI methods. Use ctx.Files.ListFiles instead of ListFilesAsync."),
            new("ListDirectoriesAsync", "Dynamic capabilities must use synchronous Cyrena ABI methods. Use ctx.Files.ListDirectories instead of ListDirectoriesAsync."),
            new("CreateDirectoryAsync", "Dynamic capabilities must use synchronous Cyrena ABI methods. Use ctx.Files.CreateDirectory instead of CreateDirectoryAsync."),
            new("DeleteDirectoryAsync", "Dynamic capabilities must use synchronous Cyrena ABI methods. Use ctx.Files.DeleteDirectory instead of DeleteDirectoryAsync."),
            new("ReadAsync", "Dynamic capabilities must use synchronous Cyrena ABI methods. Use ctx.Documents.Read instead of ReadAsync."),
            new("WriteAsync", "Dynamic capabilities must use synchronous Cyrena ABI methods. Use ctx.Documents.Write instead of WriteAsync."),
            new("AppendAsync", "Dynamic capabilities must use synchronous Cyrena ABI methods. Use ctx.Documents.Append instead of AppendAsync."),
            new("ListAsync", "Dynamic capabilities must use synchronous Cyrena ABI methods. Use ctx.Documents.List instead of ListAsync."),
            new("GetInfoAsync", "Dynamic capabilities must use synchronous Cyrena ABI methods. Use ctx.Documents.GetInfo instead of GetInfoAsync."),
        };

        public Task<ScriptValidationResult> ValidateAsync(string code, CancellationToken cancellationToken = default)
        {
            var violations = new List<ScriptViolation>();
            var lines = code.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                var trimmed = line.Trim();

                // Skip comments for directive checks but not for code checks
                var codePart = trimmed;
                if (trimmed.StartsWith("//"))
                {
                    codePart = string.Empty;
                }
                else if (trimmed.Contains("//"))
                {
                    codePart = trimmed.Substring(0, trimmed.IndexOf("//")).Trim();
                }

                // Check restricted namespaces
                foreach (var pattern in RestrictedNamespaces)
                {
                    if (codePart.Contains(pattern.Text, StringComparison.OrdinalIgnoreCase))
                    {
                        violations.Add(new ScriptViolation
                        {
                            Type = "RestrictedNamespace",
                            Pattern = pattern.Text,
                            LineNumber = i,
                            LineContent = line.Trim(),
                            Reason = pattern.Reason
                        });
                    }
                }

                // Check restricted types
                foreach (var pattern in RestrictedTypes)
                {
                    if (codePart.Contains(pattern.Text, StringComparison.OrdinalIgnoreCase))
                    {
                        violations.Add(new ScriptViolation
                        {
                            Type = "RestrictedType",
                            Pattern = pattern.Text,
                            LineNumber = i,
                            LineContent = line.Trim(),
                            Reason = pattern.Reason
                        });
                    }
                }

                // Check restricted methods
                foreach (var pattern in RestrictedMethods)
                {
                    if (codePart.Contains(pattern.Text, StringComparison.OrdinalIgnoreCase))
                    {
                        violations.Add(new ScriptViolation
                        {
                            Type = "RestrictedMethod",
                            Pattern = pattern.Text,
                            LineNumber = i,
                            LineContent = line.Trim(),
                            Reason = pattern.Reason
                        });
                    }
                }

                // Check restricted directives (check full line including comments for directives)
                foreach (var pattern in RestrictedDirectives)
                {
                    if (trimmed.StartsWith(pattern.Text, StringComparison.OrdinalIgnoreCase))
                    {
                        violations.Add(new ScriptViolation
                        {
                            Type = "RestrictedDirective",
                            Pattern = pattern.Text,
                            LineNumber = i,
                            LineContent = line.Trim(),
                            Reason = pattern.Reason
                        });
                    }
                }

                // Check restricted keywords
                foreach (var pattern in RestrictedKeywords)
                {
                    // Use word boundary check for keywords
                    var regex = new Regex($@"\b{Regex.Escape(pattern.Text)}\b", RegexOptions.IgnoreCase);
                    if (regex.IsMatch(codePart))
                    {
                        violations.Add(new ScriptViolation
                        {
                            Type = "RestrictedKeyword",
                            Pattern = pattern.Text,
                            LineNumber = i,
                            LineContent = line.Trim(),
                            Reason = pattern.Reason
                        });
                    }
                }

                // Check raw positional argument access patterns
                foreach (var pattern in RestrictedArgumentPatterns)
                {
                    if (codePart.Contains(pattern.Text, StringComparison.OrdinalIgnoreCase))
                    {
                        violations.Add(new ScriptViolation
                        {
                            Type = "RestrictedArgumentAccess",
                            Pattern = pattern.Text,
                            LineNumber = i,
                            LineContent = line.Trim(),
                            Reason = pattern.Reason
                        });
                    }
                }

                // Check async anti-patterns (dynamic capabilities should use synchronous ABI)
                foreach (var pattern in RestrictedAsyncPatterns)
                {
                    if (codePart.Contains(pattern.Text, StringComparison.OrdinalIgnoreCase))
                    {
                        violations.Add(new ScriptViolation
                        {
                            Type = "RestrictedAsyncPattern",
                            Pattern = pattern.Text,
                            LineNumber = i,
                            LineContent = line.Trim(),
                            Reason = pattern.Reason
                        });
                    }
                }
            }

            var result = new ScriptValidationResult
            {
                IsValid = violations.Count == 0,
                Violations = violations,
                Summary = violations.Count == 0
                    ? "Dynamic capability passed all validation checks. No restricted patterns detected."
                    : $"Dynamic capability validation failed with {violations.Count} violation(s). Review and remove restricted patterns before execution."
            };

            return Task.FromResult(result);
        }
        private record RestrictedPattern(string Text, string Reason);
    }
}
