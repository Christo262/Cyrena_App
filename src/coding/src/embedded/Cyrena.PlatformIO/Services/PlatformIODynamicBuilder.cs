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
        options.UseDynamicDiscovery<PioDynamicInitializer>();
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