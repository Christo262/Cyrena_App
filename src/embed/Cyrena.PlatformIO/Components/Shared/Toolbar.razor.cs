using Cyrena.Contracts;
using Cyrena.Developer.Contracts;
using Cyrena.Developer.Models;
using Cyrena.Developer.Options;
using Cyrena.PlatformIO.Contracts;
using Cyrena.PlatformIO.Extensions;
using Cyrena.Developer.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Cyrena.PlatformIO.Components.Shared
{
    public partial class Toolbar
    {
        private IEnvironmentController _environment = default!;
        private IDevelopPlanService _plan = default!;
        private IChatConfigurationService _config = default!;
        private IIterationService _its = default!;

        private string? _name { get; set; }
        protected override void OnInitialized()
        {
            _environment = Kernel.Services.GetRequiredService<IEnvironmentController>();
            _plan = Kernel.Services.GetRequiredService<IDevelopPlanService>();
            _config = Kernel.Services.GetRequiredService<IChatConfigurationService>();
            _its = Kernel.Services.GetRequiredService<IIterationService>();
            _name = _environment.Current?.Name;
        }

        private void OnChange()
        {
            if (_environment.Current!.Framework?
                .Split(',', StringSplitOptions.TrimEntries)
                .Any(f => f.Equals("espidf", StringComparison.OrdinalIgnoreCase)) == true)
            {
                _plan.Plan.IndexPlatformIOEspIdf();

                var envName = _environment.Current.Name.Replace("env:", "");
                var sdkName = $"sdkconfig.{envName}";
                var sdkPath = Path.Combine(_config.Config[DevelopOptions.RootDirectory]!, sdkName);

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
