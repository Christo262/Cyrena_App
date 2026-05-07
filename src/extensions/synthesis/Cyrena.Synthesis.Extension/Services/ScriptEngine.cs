using Cyrena.Synthesis.Contracts;
using Cyrena.Synthesis.Models;
using Cyrena.Synthesis.Options;
using FSharp.Compiler.CodeAnalysis;
using FSharp.Compiler.Diagnostics;
using Microsoft.FSharp.Control;
using Microsoft.FSharp.Core;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace Cyrena.Synthesis.Services
{
    internal class ScriptEngine : IScriptEngine
    {
        private readonly SynthesisOptions _options;
        private readonly IServiceProvider _services;
        private readonly IScriptValidator _validator;
        private readonly ICapabilityPermissionService _permissionService;
        private readonly SynthesisBuilder _builder;
        private readonly ICapabilityContext _context;

        public ScriptEngine(SynthesisOptions options, IServiceProvider services, IScriptValidator validator, ICapabilityPermissionService permissionService, SynthesisBuilder builder, ICapabilityContext context)
        {
            _options = options;
            _services = services;
            _validator = validator;
            _permissionService = permissionService;
            _builder = builder;
            _context = context;
        }

        public async Task<CapabilityExecutionResult> ExecuteAsync(DynamicCapability script, CapabilityRequest request, CancellationToken cancellationToken = default)
        {
            _context.SetCurrent(script);
            var stopwatch = Stopwatch.StartNew();
            var result = new CapabilityExecutionResult
            {
                ScriptId = script.Id,
                Arguments = request.ToArgumentDictionary(),
                ExecutedAt = DateTime.UtcNow
            };

            try
            {
                // Step 1: Validate dynamic capability for restricted patterns (guardrail)
                if (_options.ValidateScriptsBeforeExecution && request.ValidateBeforeExecution)
                {
                    var validationResult = await _validator.ValidateAsync(script.Code, cancellationToken);
                    if (!validationResult.IsValid)
                    {
                        var violationSummary = string.Join("\n", validationResult.Violations.Select(v =>
                            $"  Line {v.LineNumber + 1}: [{v.Type}] {v.Pattern} - {v.Reason}"));

                        if (_options.RejectInvalidScripts)
                        {
                            result.Success = false;
                            result.Error = $"Dynamic capability validation failed. Restricted patterns detected:\n{violationSummary}\n\n" +
                                "Dynamic capabilities must use Cyrena APIs (ctx.Files, ctx.Log, ctx.Args) instead of direct system access. " +
                                "Use synchronous methods: ctx.Files.ReadText, ctx.Files.WriteText. " +
                                "Never use raw positional indexing like args.[0], Async.AwaitTask, or Async.RunSynchronously.";
                            result.ExecutionTime = stopwatch.Elapsed;
                            return result;
                        }
                        else
                        {
                            result.Output = $"WARNING: Dynamic capability validation detected restricted patterns:\n{violationSummary}\n";
                        }
                    }
                }

                // Step 2: Check permissions
                if (!await _context.RequestPermissionAsync(script, new CapabiliyPermissionDescriptor("Script.Execute", "execute script")))
                {
                    result.Success = false;
                    result.Error = $"Dynamic capability '{script.Title}' does not have execution permission.";
                    result.ExecutionTime = stopwatch.Elapsed;
                    return result;
                }
                var grantedPermissions = await _permissionService.GetGrantedPermissionsAsync(script.Id, cancellationToken);
                var permissionNames = grantedPermissions.Select(p => p.PermissionName).ToList();
                result.ActivePermissions = permissionNames.ToList();

                // Step 3: Build structured arguments from the request
                var argsDictionary = request.ToArgumentDictionary();
                var cyrenaArgs = new CapabilityArgs(argsDictionary);

                // Step 4: Build execution context with capability gating
                var logger = new CapabilityLogger(_context);
                var context = BuildScriptContext(script.Id, cyrenaArgs, logger, permissionNames);

                // Step 5: Compile the dynamic capability with restricted references
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(request.Timeout ?? _options.MaxExecutionTime);

                var compileResult = await CompileScriptAsync(script, cancellationToken);
                if (!compileResult.Success)
                {
                    result.Success = false;
                    result.Error = compileResult.Error;
                    result.ExecutionTime = stopwatch.Elapsed;
                    return result;
                }

                // Step 6: Execute the compiled dynamic capability with the Cyrena context
                var executionResult = await ExecuteCompiledScriptAsync(
                    compileResult.AssemblyBytes!,
                    context,
                    cts.Token);

                result.Success = executionResult.Success;
                result.Output = executionResult.Output + "\n" + logger.CapturedOutput;
                result.Error = executionResult.Error;
                result.ReturnValue = executionResult.ReturnValue;
                result.ExecutionTime = stopwatch.Elapsed;

                return result;
            }
            catch (OperationCanceledException)
            {
                result.Success = false;
                result.Error = $"Dynamic capability execution was cancelled or exceeded the maximum execution time of {_options.MaxExecutionTime}.";
                result.ExecutionTime = stopwatch.Elapsed;
                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Error = $"Execution error: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}";
                result.ExecutionTime = stopwatch.Elapsed;
                return result;
            }
        }

        public async Task<CapabilityExecutionResult> ValidateAsync(string code, CancellationToken cancellationToken = default)
        {
            // First run pattern validation
            var validationResult = await _validator.ValidateAsync(code, cancellationToken);
            if (!validationResult.IsValid && _options.RejectInvalidScripts)
            {
                var violationSummary = string.Join("\n", validationResult.Violations.Select(v =>
                    $"  Line {v.LineNumber + 1}: [{v.Type}] {v.Pattern} - {v.Reason}"));

                return new CapabilityExecutionResult
                {
                    Success = false,
                    Error = $"Dynamic capability validation failed. Restricted patterns detected:\n{violationSummary}",
                    ExecutedAt = DateTime.UtcNow
                };
            }

            // Then try compilation
            var script = new DynamicCapability
            {
                Id = Guid.NewGuid().ToString("N"),
                Title = "Validation",
                Code = code,
                CreatedAt = DateTime.UtcNow,
                ModifiedAt = DateTime.UtcNow
            };

            var compileResult = await CompileScriptAsync(script, cancellationToken);
            return new CapabilityExecutionResult
            {
                Success = compileResult.Success,
                Error = compileResult.Error,
                Output = compileResult.Success
                    ? "Dynamic capability compiled successfully."
                    : $"Compilation failed:\n{compileResult.Error}",
                ExecutedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Builds a fully wired ICyrenaScriptContext with capability-gated APIs.
        /// </summary>
        private ICapabilityExecutionContext BuildScriptContext(
            string scriptId,
            ICapabilityArgs args,
            ICapabilityLogger logger,
            IEnumerable<string> permissionNames)
        {
            var context = new CapabilityExecutionContext(_services, args, logger, _builder.CapabilityAbis);
            return context;
        }

        /// <summary>
        /// Resolves the directory containing the framework assemblies, with fallbacks
        /// for self-contained deployments where there is no system-wide .NET install.
        ///
        /// On a self-contained deployment typeof(object).Assembly.Location points into
        /// the app's own publish directory, which is exactly what we want — fsc must
        /// compile against the same assemblies the runtime will load.
        ///
        /// AppContext.BaseDirectory is used as a fallback for single-file publish
        /// scenarios where Assembly.Location may return an empty string (the assembly
        /// is bundled inside the executable and extracted to a temp directory at
        /// startup; BaseDirectory reflects that extraction root).
        /// </summary>
        private static string GetFrameworkDirectory()
        {
            // Primary: location of the assembly that defines System.Object.
            // In a self-contained app this is inside the publish / extraction dir.
            var fromObject = Path.GetDirectoryName(typeof(object).Assembly.Location);
            if (!string.IsNullOrEmpty(fromObject) && Directory.Exists(fromObject))
                return fromObject;

            // Fallback for single-file publish: the extraction root always contains
            // the unbundled framework assemblies.
            return AppContext.BaseDirectory;
        }

        /// <summary>
        /// Compiles an F# dynamic capability with restricted references.
        /// Only approved assemblies are exposed to the dynamic capability.
        ///
        /// --noframework + --simpleresolution is used so that:
        ///   - fsc does not try to locate the SDK / MSBuild reference packs (safe for
        ///     self-contained deployments with no system .NET install).
        ///   - fsc resolves assembly names by simple filename lookup inside the
        ///     directories supplied via --lib:, rather than attempting SDK resolution.
        ///   - System.Private.CoreLib is never passed as an explicit -r: reference.
        ///     The compiler sees System.Runtime (the facade) as authoritative and
        ///     handles the type-forward relationship to CoreLib internally, avoiding
        ///     the "type found in X but not in System.Runtime" unification errors.
        /// </summary>
        private async Task<CompileResult> CompileScriptAsync(DynamicCapability script, CancellationToken cancellationToken)
        {
            var checker = FSharpChecker.Create(
                FSharpOption<int>.Some(1),
                FSharpOption<bool>.Some(true),
                FSharpOption<bool>.Some(false),
                FSharpOption<LegacyReferenceResolver>.None,
                FSharpOption<FSharpFunc<Tuple<string, DateTime>, FSharpOption<Tuple<object, nint, int>>>>.None,
                FSharpOption<bool>.None,
                FSharpOption<bool>.None,
                FSharpOption<bool>.None,
                FSharpOption<bool>.None,
                FSharpOption<bool>.None,
                FSharpOption<bool>.None,
                FSharpOption<DocumentSource>.None,
                FSharpOption<bool>.None,
                FSharpOption<FSharp.Compiler.CodeAnalysis.TransparentCompiler.CacheSizes>.None);

            var tempDir = Path.Combine(Path.GetTempPath(), "cyrena-fx", script.Id);
            Directory.CreateDirectory(tempDir);

            var sourceFile = Path.Combine(tempDir, $"script_{script.Id}.fsx");
            var outputFile = Path.Combine(tempDir, $"script_{script.Id}.dll");

            await File.WriteAllTextAsync(sourceFile, script.Code, cancellationToken);

            var frameworkDir = GetFrameworkDirectory();

            var compileArgs = new List<string>
            {
                "fsc.exe",
                $"-o:{outputFile}",
                "--noframework",
                // --simpleresolution tells fsc to resolve assembly references by
                // filename inside --lib: directories instead of using MSBuild / SDK
                // resolution. This is required for self-contained deployments.
                "--simpleresolution",
                "--target:library",
                "--platform:anycpu",
                "--debug-",
                "--optimize+",
                "--tailcalls+",
                "--nowarn:FS0988",
                // Point fsc at the framework directory so it can locate assemblies
                // passed as -r: by filename without needing full paths for each one.
                $"--lib:{frameworkDir}",
                sourceFile
            };

            // Add approved references by filename only (resolved via --lib: above).
            // System.Private.CoreLib is intentionally excluded: passing it as an
            // explicit -r: alongside System.Runtime causes type-unification errors
            // because the compiler sees two definitions of every primitive type.
            // With --simpleresolution, fsc handles the CoreLib / Runtime relationship
            // correctly on its own once System.Runtime.dll is referenced.
            foreach (var reference in _options.ApprovedReferences)
            {
                var refPath = Path.Combine(frameworkDir, reference);
                if (File.Exists(refPath))
                    compileArgs.Add($"-r:{reference}");
            }

            //// Add Cyrena.Synthesis.Contracts assembly for ABI access.
            //// These are app assemblies (not framework), so they need full paths.
            //var contractsAssembly = typeof(ICapabilityExecutionContext).Assembly;
            //var contractsPath = ResolveAssemblyPath(contractsAssembly);
            //if (!string.IsNullOrEmpty(contractsPath))
            //    compileArgs.Add($"-r:{contractsPath}");

            // Add FSharp.Core (full path — not in frameworkDir).
            var fsharpCorePath = ResolveAssemblyPath(typeof(FSharpOption<>).Assembly);
            if (!string.IsNullOrEmpty(fsharpCorePath))
                compileArgs.Add($"-r:{fsharpCorePath}");

            // Add ABI assemblies (full paths).
            var abiAssemblies = _builder.CapabilityAbis.Select(x => x.ServiceType.Assembly).Distinct();
            foreach (var item in abiAssemblies)
            {
                var abiPath = ResolveAssemblyPath(item);
                if (!string.IsNullOrEmpty(abiPath))
                    compileArgs.Add($"-r:{abiPath}");
            }

            // Compile
            var compilationResult = await FSharpAsync.StartAsTask(
                checker.Compile(compileArgs.ToArray(), FSharpOption<string>.None),
                FSharpOption<TaskCreationOptions>.None,
                FSharpOption<CancellationToken>.Some(cancellationToken));

            var errors = compilationResult.Item1
                .Where(d => d.Severity == FSharpDiagnosticSeverity.Error)
                .ToList();

            if (errors.Count > 0)
            {
                var errorBuilder = new StringBuilder();
                foreach (var error in errors)
                {
                    errorBuilder.AppendLine($"[{error.Range.StartLine}:{error.Range.StartColumn}] {error.Message}");
                }

                return new CompileResult
                {
                    Success = false,
                    Error = errorBuilder.ToString()
                };
            }

            if (!File.Exists(outputFile))
            {
                return new CompileResult
                {
                    Success = false,
                    Error = "Compilation succeeded but output assembly was not created."
                };
            }

            var assemblyBytes = await File.ReadAllBytesAsync(outputFile, cancellationToken);
            return new CompileResult
            {
                Success = true,
                AssemblyBytes = assemblyBytes
            };
        }

        private static string? ResolveAssemblyPath(Assembly assembly)
        {
            // Try the assembly's own location first
            if (!string.IsNullOrEmpty(assembly.Location) && File.Exists(assembly.Location))
                return assembly.Location;

            // Fallback: search loaded assemblies by full name or simple name.
            // Handles single-file publish where Assembly.Location may be empty.
            var simpleName = assembly.GetName().Name;
            var loaded = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => !string.IsNullOrEmpty(a.Location) &&
                    (a.FullName == assembly.FullName || a.GetName().Name == simpleName));

            if (loaded != null && File.Exists(loaded.Location))
                return loaded.Location;

            return null;
        }

        /// <summary>
        /// Executes a compiled dynamic capability assembly with the Cyrena runtime context.
        /// The dynamic capability entry point must accept ICapabilityExecutionContext.
        ///
        /// Example entry point:
        ///   let main (ctx: ICapabilityExecutionContext) =
        ///       let files = ctx.GetRequiredService&lt;IFileSystemAbi&gt;()
        ///       let filePath = ctx.Args.GetString("filePath")
        ///       let text = files.ReadText(filePath)
        ///       files.WriteText(filePath, text + "\nUpdated")
        ///       ctx.Log.Info("File updated successfully")
        /// </summary>
        private async Task<CapabilityExecutionResult> ExecuteCompiledScriptAsync(
            byte[] assemblyBytes,
            ICapabilityExecutionContext context,
            CancellationToken cancellationToken)
        {
            var result = new CapabilityExecutionResult
            {
                Success = true,
                ExecutedAt = DateTime.UtcNow
            };

            var alc = new ScriptAssemblyLoadContext();

            // Preload approved framework assemblies into the ALC so that type identity
            // is consistent between the compiled script and the host process.
            // Uses the same GetFrameworkDirectory() logic as compilation so the ALC
            // resolves against the exact same files fsc compiled against.
            var frameworkDir = GetFrameworkDirectory();
            foreach (var reference in _options.ApprovedReferences)
            {
                var refPath = Path.Combine(frameworkDir, reference);
                if (File.Exists(refPath))
                    alc.PreloadAssembly(refPath);
            }

            alc.PreloadAssembly("FSharp.Core", typeof(FSharpOption<>).Assembly);
            //alc.PreloadAssembly("Cyrena.Synthesis.Core", typeof(ICapabilityExecutionContext).Assembly);

            var abiAssemblies = _builder.CapabilityAbis.Select(x => x.ServiceType.Assembly).Distinct();
            foreach (var item in abiAssemblies)
                alc.PreloadAssembly(item.GetName().Name!, item);

            var assembly = alc.LoadFromBytes(assemblyBytes);

            // Find the entry point - look for a static 'main' accepting ICapabilityExecutionContext
            var entryPoint = assembly.GetTypes()
                .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Static))
                .FirstOrDefault(m =>
                {
                    var parameters = m.GetParameters();
                    return m.Name.Equals("main", StringComparison.OrdinalIgnoreCase) &&
                           parameters.Length == 1 &&
                           parameters[0].ParameterType == typeof(ICapabilityExecutionContext);
                });

            if (entryPoint == null)
            {
                // Fallback: any static method accepting ICapabilityExecutionContext
                entryPoint = assembly.GetTypes()
                    .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Static))
                    .FirstOrDefault(m =>
                    {
                        var parameters = m.GetParameters();
                        return parameters.Length == 1 &&
                               parameters[0].ParameterType == typeof(ICapabilityExecutionContext);
                    });
            }

            if (entryPoint == null)
            {
                result.Success = false;
                result.Error = "No suitable entry point found in the compiled dynamic capability. " +
                    "Ensure the dynamic capability contains a static 'main' function accepting ICapabilityExecutionContext.\n\n" +
                    "Example entry point:\n" +
                    "open Cyrena.Synthesis.Contracts\n\n" +
                    "let main (ctx: ICapabilityExecutionContext) =\n" +
                    "    let files = ctx.GetRequiredService<IFileSystemAbi>()\n" +
                    "    let filePath = ctx.Args.GetString(\"filePath\")\n" +
                    "    let text = files.ReadText(filePath)\n" +
                    "    files.WriteText(filePath, text + \"\\nUpdated\")\n" +
                    "    ctx.Log.Info(\"File updated successfully\")";
                return result;
            }

            try
            {
                var invokeResult = entryPoint.Invoke(null, new object[] { context });

                if (invokeResult != null)
                {
                    // Handle async results
                    if (invokeResult is Task task)
                    {
                        await task.WaitAsync(cancellationToken);

                        // Try to get result value from Task<T>
                        var taskType = task.GetType();
                        if (taskType.IsGenericType)
                        {
                            var resultProperty = taskType.GetProperty("Result");
                            if (resultProperty != null)
                            {
                                var taskResult = resultProperty.GetValue(task);
                                if (taskResult != null)
                                    result.ReturnValue = taskResult.ToString();
                            }
                        }
                    }
                    else
                    {
                        result.ReturnValue = invokeResult.ToString();
                    }
                }

                result.Results = context.Result.ResultBag.Values.ToDictionary(
                    x => x.Key,
                    x => x.Value,
                    StringComparer.OrdinalIgnoreCase);
                result.Success = true;
            }
            catch (TargetInvocationException tie) when (tie.InnerException != null)
            {
                var real = tie.InnerException;
                result.Success = false;
                result.Error = $"Dynamic capability runtime error: {real.GetType().Name}: {real.Message}\n{real.StackTrace}";
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Error = $"Dynamic capability execution error: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}";
            }

            return result;
        }
    }
}