using Cyrena.Coding.Contracts;
using Cyrena.Coding.Extensions;
using Cyrena.Coding.Models;
using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Models;
using Cyrena.PlatformIO.Components.Shared;
using Cyrena.PlatformIO.Contracts;
using Cyrena.PlatformIO.Models;
using Cyrena.PlatformIO.Options;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;

namespace Cyrena.PlatformIO.Services;

public class PlatformIODynamicBuilder : ICodeBuilder
{
    private readonly IKernelController _kernel;
    public PlatformIODynamicBuilder(IKernelController kernel)
    {
        _kernel = kernel;
    }
    
    public Task<DevelopPlan> ConfigureAsync(CyrenaKernelBuilder options)
    {
        var environments = PlatformIOEnvironment.Parse(options.ChatConfiguration[PlatformIOOptions.IniFile] ?? throw new NullReferenceException("platformio.ini not set"));
        if (!environments.Any())
            throw new InvalidOperationException("No environments defined in platformio.ini");
        var plan = new DevelopPlan(options.ChatConfiguration.WorkingDirectory ?? Path.GetDirectoryName(options.ChatConfiguration[PlatformIOOptions.IniFile])!);
        IEnvironmentController environmentController =
            new EnvironmentController(environments);
        environmentController.SetCurrentEnvironment(environments[0].Name);
        options.Services.AddSingleton<IEnvironmentController>(environmentController);
        options.UseDynamicDiscovery(plan =>
        {
            plan.Discover("ini", true, true);
            var src = plan.GetOrCreateFolder("src", "src");
            plan.Discover(src, "c",false);
            plan.Discover(src, "cpp",false);
            plan.Discover(src, "h",false);
            plan.Discover(src, "hpp",false);
            
            var include =  plan.GetOrCreateFolder("include", "include");
            plan.Discover(include, "c",false);
            plan.Discover(include, "cpp",false);
            plan.Discover(include, "h",false);
            plan.Discover(include, "hpp",false);
            
            var data = plan.GetOrCreateFolder("data", "data");
            plan.Discover(data, "txt",false);
            plan.Discover(data, "json",false);

            string[] readFolders = ["lib"];
            if (environments.Any(env => env.Framework?.Split(',', StringSplitOptions.TrimEntries)
                    .Any(f => f.Equals("espidf", StringComparison.OrdinalIgnoreCase)) == true))
            {
                readFolders = ["lib", "components", "managed_components"];
            }

            foreach (var rd in readFolders)
            {
                var lib = plan.GetOrCreateFolder(rd, rd);
                if (Directory.Exists(Path.Combine(plan.RootDirectory, lib.RelativePath)))
                {
                    var libs = Directory.GetDirectories(Path.Combine(plan.RootDirectory, lib.RelativePath));
                    foreach (var item in libs)
                    {
                        var inf = new DirectoryInfo(item);
                        var lbFolder = plan.GetOrCreateFolder(lib, $"{rd}_{inf.Name.ToLower()}", inf.Name);
                        plan.IndexFiles(lbFolder,"h", $"{lbFolder.Id}_",true);
                        if (Directory.Exists(Path.Combine(plan.RootDirectory, lbFolder.RelativePath, "src")))
                        {
                            var lbSrc = plan.GetOrCreateFolder(lbFolder, $"{lbFolder.Id}_src", "src");
                            plan.IndexFiles(lbSrc,"h", $"{lbSrc.Id}_",true);
                        }
                        if (Directory.Exists(Path.Combine(plan.RootDirectory, lbFolder.RelativePath, "include")))
                        {
                            var lbInclude = plan.GetOrCreateFolder(lbFolder, $"{lbFolder.Id}_include", "include");
                            plan.IndexFiles(lbInclude,"h", $"{lbInclude.Id}_",true);
                        }
                    }
                }
            }
        });
        var prompt = Resources.Read(typeof(PlatformIOBuilder).Assembly, "Cyrena.PlatformIO.Resources.dynamic.prompt.md");
        options.GetFeatureOption<IPromptManager>().AddPrompt(0, prompt);
        options.AddToolbarComponent<Cyrena.PlatformIO.Components.Shared.Toolbar>(ToolbarAlignment.Start);
        return Task.FromResult(plan);
    }

    public Task DeleteAsync(ChatConfiguration config)
    {
        return Task.CompletedTask;
    }

    public async Task EditAsync(ChatConfiguration config, IServiceProvider services)
    {
        var dialogService = services.GetRequiredService<IDialogService>();
        var parameters = new DialogParameters<Configure>
        {
            { x => x.Model, config }
        };
        var options = new DialogOptions { MaxWidth = MaxWidth.Small };
        var dialog = await dialogService.ShowAsync<Configure>("Dynamic PlatformIO", parameters, options);
        var result = await dialog.Result;

        if (result is not null && !result.Canceled)
            await _kernel.UpdateAsync(config, true);
    }

    public string Id { get; } = "platformio.dynamic";
}