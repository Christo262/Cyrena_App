using Cyrena.CLI.Attributes;
using Cyrena.CLI.Models;
using Cyrena.Extensions;
using Cyrena.Options;
using Cyrena.Shell.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Cyrena.Shell.Services
{
    [CliSurface]
    public class CliCommands
    {
        [CliCommand("kill")]
        public async Task<CliExecutionResult> Kill()
        {
            var settings = CyrenaRuntime.CreateSettings();
            var options = settings.Read<ApplicationOptions>(ApplicationOptions.Key);
            if (options == null)
                return new CliExecutionResult()
                {
                    ShouldContinueBoot = false,
                    Message = "No application configuration found.",
                    ExitCode = 0,
                };

            var squawk = settings.Read<Squawk>(Squawk.Key);
            if(squawk == null)
            {
                squawk = new Squawk();
                settings.Save(Squawk.Key, squawk);
            }

            try
            {
                using var http = new HttpClient()
                {
                    BaseAddress = new Uri($"http://localhost:{options.ServerPort}")
                };
                using var response = await http.GetAsync($"/api/kill?squawk={squawk.Value}");
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized || response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    return new CliExecutionResult()
                    {
                        ExitCode = 1,
                        Message = "Unable to close (FORBIDDEN)",
                        ShouldContinueBoot = false,
                    };
                if(response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                    return new CliExecutionResult()
                    {
                        ExitCode = 1,
                        Message = "Unable to close (ERROR)",
                        ShouldContinueBoot = false,
                    };
                return new CliExecutionResult()
                {
                    Message = "Cyréna closed",
                    ShouldContinueBoot = false
                };
            }catch (Exception ex)
            {
                return new CliExecutionResult()
                {
                    ExitCode = 1,
                    ShouldContinueBoot= false,
                    Message= ex.Message,
                };
            }
        }

        [CliCommand("set")]
        public CliExecutionResult Set(
            [CliParam("port", DefaultValue = null, Description = "Specify the port number to use for Cyréna's background process.")]int? port,
            [CliParam("launch-window", DefaultValue = null, Description = "If Shell window should be launched on startup (true or false).")]bool? launchWindow)
        {
            var settings = CyrenaRuntime.CreateSettings();
            var options = settings.Read<ApplicationOptions>(ApplicationOptions.Key) ?? new ApplicationOptions();
            var changes = new List<string>();
            if (port.HasValue)
            {
                if(port < 1000 || port > 10000)
                {
                    return new CliExecutionResult()
                    {
                        ExitCode = 1,
                        Message = "Invalid --port range. Min 1000, max 10000",
                        ShouldContinueBoot = false
                    };
                }
                options.ServerPort = port.Value;
                changes.Add($"Set port {options.ServerPort}");
            }

            if (launchWindow.HasValue)
            {
                options.LaunchWindowOnStartup = launchWindow.Value;
                changes.Add($"Set launch window: {options.LaunchWindowOnStartup}");
            }

            if(changes.Any())
                settings.Save<ApplicationOptions>(ApplicationOptions.Key, options);
            return new CliExecutionResult()
            {
                ExitCode = 0,
                ShouldContinueBoot = false,
                Message = $"Changes made:\n {string.Join("\n\t", changes)}"
            };
        }
    }
}
