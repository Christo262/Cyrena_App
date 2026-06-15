using Cyrena.Coding.Contracts;
using Cyrena.Coding.Models;
using Cyrena.Contracts;
using Cyrena.Dotnet.Contracts;
using Cyrena.Extensions;
using Cyrena.Models;
using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Cyrena.VisualStudio.Services
{
    internal class DotnetFunctions
    {
        private readonly ISolutionController _sln;
        private readonly IChatMessageService _chat;
        private readonly IDevelopPlanService _plan;
        public DotnetFunctions(ISolutionController sln, IChatMessageService chat, IDevelopPlanService plan)
        {
            _sln = sln;
            _chat = chat;
            _plan = plan;
        }

        [KernelFunction("build")]
        [Description("Runs dotnet build in the project directory and returns output and errors.")]
        public ToolResult<ConsoleOutput> RunDotnetBuild()
        {
            try
            {
                const string arguments = "build";
                _chat.LogInfo($"Running dotnet {arguments} in {_plan.Plan.RootDirectory}...");

                var info = new ProcessStartInfo(GetDotnetExecutable(), arguments)
                {
                    WorkingDirectory = _plan.Plan.RootDirectory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                var output = new ConsoleOutput()
                {
                    Command = $"dotnet {arguments}"
                };

                using var process = Process.Start(info);
                if (process == null)
                    return new ToolResult<ConsoleOutput>(false, "Unable to start dotnet. Verify installation.");

                process.OutputDataReceived += (_, e) =>
                {
                    if (e.Data != null)
                    {
                        output.WriteLine("info", e.Data);
                        _chat.LogInfo($"\t{e.Data}");
                    }
                };
                process.ErrorDataReceived += (_, e) =>
                {
                    if (e.Data != null)
                    {
                        output.WriteLine("error", e.Data);
                        _chat.LogError($"\t{e.Data}");
                    }
                };

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
                process.WaitForExit(); // flush buffers

                const string lineNumberWarning = "IMPORTANT: Line numbers in build output refer to generated code, not source files — do not use them to locate code in .razor or .cs files. Search for the relevant code by name or pattern instead.";

                if (process.ExitCode != 0)
                    return new ToolResult<ConsoleOutput>(output, false, $"dotnet {arguments} failed. {lineNumberWarning}");

                return new ToolResult<ConsoleOutput>(output, true, $"dotnet {arguments} succeeded. {lineNumberWarning}");
            }
            catch (Exception ex)
            {
                _chat.LogError(ex.Message);
                return new ToolResult<ConsoleOutput>(false, ex.Message);
            }
        }

        private string GetDotnetExecutable()
        {
            // 1. Check if 'dotnet' is already in the current process PATH
            // On Windows, this usually works. On Linux, it depends on how the app was launched.
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return "dotnet";
            }

            // 2. For Linux/macOS, we need to be more explicit.
            // We check common installation paths if the simple "dotnet" call fails.
            string[] commonPaths = { "/usr/bin/dotnet", "/usr/local/bin/dotnet", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".dotnet/dotnet") };

            foreach (var path in commonPaths)
            {
                if (File.Exists(path)) return path;
            }

            return "dotnet"; // Fallback to default
        }
    }
}
