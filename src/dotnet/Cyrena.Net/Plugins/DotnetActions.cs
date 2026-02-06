using Cyrena.Contracts;
using Cyrena.Models;
using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Diagnostics;

namespace Cyrena.Net.Plugins
{
    public class DotnetActions
    {
        private readonly IDeveloperContext _context;
        public DotnetActions(IDeveloperContext context)
        {
            _context = context;
        }

        [KernelFunction]
        [Description("Gets the root namespace and target framework of the project.")]
        public ToolResult<string> GetRootNamespaceAndTargetFramework()
        {
            var ns = _context.Project.Properties["namespace"];
            var tf = _context.Project.Properties["targetFramework"];
            if (string.IsNullOrEmpty(ns) || string.IsNullOrEmpty(tf))
                return new ToolResult<string>(false, "Project configuration is incomplete.");

            var str = $"Namespace: {ns}\r\nTarget Framework: {tf}";
            return new ToolResult<string>(str, true);
        }

#warning TODO this is a bad idea, need to whitelist commands it can run. Maybe a config file with actual actions that can be done
        [KernelFunction]
        [Description("Runs a dotnet CLI command in the project directory and returns output and errors.")]
        public ToolResult<string[]> RunDotnetCommand(
            [Description("Arguments passed to dotnet CLI, for example: \"build\", \"test\", \"publish -c Release\"")]
    string arguments)
        {
            _context.LogInfo($"Running dotnet {arguments} ...");

            // Basic safety guard
            if (string.IsNullOrWhiteSpace(arguments))
                return new ToolResult<string[]>(false, "Arguments cannot be empty.");

            var info = new ProcessStartInfo("dotnet", arguments)
            {
                WorkingDirectory = _context.Project.RootDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(info);
            if (process == null)
                return new ToolResult<string[]>(false, "Unable to start dotnet. Verify installation.");

            var logs = new List<string>();
            var errors = new List<string>();

            process.OutputDataReceived += (_, e) =>
            {
                if (e.Data != null)
                {
                    logs.Add(e.Data);
                    _context.LogInfo($"\t{e.Data}");
                }
            };

            process.ErrorDataReceived += (_, e) =>
            {
                if (e.Data != null)
                {
                    errors.Add(e.Data);
                    _context.LogError($"\t{e.Data}");
                }
            };

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            process.WaitForExit();
            process.WaitForExit(); // flush buffers

            if (process.ExitCode != 0)
            {
                return new ToolResult<string[]>(
                    errors.Concat(logs).ToArray(),
                    false,
                    $"dotnet {arguments} failed"
                );
            }

            return new ToolResult<string[]>(
                logs.ToArray(),
                true,
                $"dotnet {arguments} succeeded"
            );
        }
    }
}
