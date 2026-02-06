using Microsoft.SemanticKernel;
using Cyrena.Contracts;
using Cyrena.Models;
using System.ComponentModel;
using System.Diagnostics;

namespace Cyrena.Blazor.Plugins
{
    public class BlazorActions
    {
        private readonly IDeveloperContext _context;
        public BlazorActions(IDeveloperContext context)
        {
            _context = context;
        }

        [KernelFunction]
        [Description("Builds the project and returns build status and any compiler errors.")]
        public ToolResult<string[]> BuildProject()
        {
            _context.LogInfo("Building project...");
            var info = new ProcessStartInfo("dotnet", "build")
            {
                WorkingDirectory = _context.Project.RootDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(info);
            if (process == null)
                return new ToolResult<string[]>(false, "Unable to start process. Verify dotnet installation.");

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
            process.WaitForExit(); // ensure buffers flushed

            if (process.ExitCode != 0)
            {
                return new ToolResult<string[]>(
                    errors.Concat(logs).ToArray(),
                    false,
                    "Command failed, see Result."
                );
            }

            return new ToolResult<string[]>(
                logs.ToArray(),
                true,
                "Command run, see Result."
            );
        }

    }
}
