using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Models;
using Cyrena.Screens.Components.Shared;
using Cyrena.Screens.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

namespace Cyrena.Screens.Services;

internal class AssistantPlugin : IAssistantPlugin
{
    private readonly IScreenInterop _interop;

    public AssistantPlugin(IScreenInterop interop)
    {
        _interop = interop;
    }
    
    public string Id { get; } = "cyrena.screens";
    public string[] Modes { get; } = [];
    public int Priority { get; } = 100;
    public bool Required { get; } = false;
    public string Title { get; } = "Screen Share";
    
    public Task LoadAsync(CyrenaKernelBuilder builder)
    {
        var info = builder.GetFeatureOption<ConnectionInfo>();
        if (!info.SupportImages)
            return Task.CompletedTask;
        
        builder.AddToolbarComponent<ScreenShareTool>(ToolbarAlignment.End);
        builder.Services.AddSingleton<IScreenInterop>(_interop);
        builder.Plugins.AddFromType<Functions>("Screen");

        var prompts = builder.GetFeatureOption<IPromptManager>();
        prompts.AddPrompt(20, Resources.Read(typeof(AssistantPlugin).Assembly, "Cyrena.Screens.Resources.prompt.md"));
        return Task.CompletedTask;
    }
}