using Cyrena.Contracts;
using Cyrena.Coding.Contracts;
using Cyrena.Coding.Models;
using Cyrena.Coding.Options;
using Cyrena.PlatformIO.Contracts;
using Cyrena.PlatformIO.Extensions;
using Cyrena.Coding.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Cyrena.Attributes;

namespace Cyrena.PlatformIO.Components.Shared
{
    public partial class Toolbar
    {
        [KernelInject]
        private IEnvironmentController _environment { get; set; } = default!;
        [KernelInject]
        private IDevelopPlanService _plan { get; set; } = default!;
        [KernelInject]
        private IChatConfigurationService _config { get; set; } = default!;
        [KernelInject]
        private IIterationService _its { get; set; } = default!;

        private string? _name { get; set; }
        protected override void OnInitialized()
        {
            _name = _environment.Current?.Name;
        }

        private void OnChange(string? e)
        {
            _name = e;
            if (_environment.Current!.Framework?
                .Split(',', StringSplitOptions.TrimEntries)
                .Any(f => f.Equals("espidf", StringComparison.OrdinalIgnoreCase)) == true)
            {
                _plan.Plan.IndexPlatformIOEspIdf();

                var envName = _environment.Current.Name.Replace("env:", "");
                var sdkName = $"sdkconfig.{envName}";
                var sdkPath = Path.Combine(_config.Config.WorkingDirectory!, sdkName);

                if (File.Exists(sdkPath) && !_plan.Plan.TryFindFileByName(sdkName, out _))
                {
                    _plan.Plan.Files.Add(new DevelopFile()
                    {
                        Id = "sdkconfig",
                        Name = sdkName,
                        RelativePath = sdkName,
                        ReadOnly = true
                    });
                }
            }
        }
    }
}
