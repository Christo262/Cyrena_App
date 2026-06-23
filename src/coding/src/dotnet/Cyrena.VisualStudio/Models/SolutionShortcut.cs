using Cyrena.Coding.Options;
using Cyrena.Contracts;
using Cyrena.Models;
using Cyrena.VisualStudio.Components.Shared;
using Cyrena.VisualStudio.Contracts;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Cyrena.VisualStudio.Models;

public class SolutionShortcut(IDialogService dialog, IKernelController kernel, NavigationManager nav) : IShortcut
{
    public string Title => "Solution";
    public string Description => "Work on multiple projects in .sln/.slnx";
    public string Icon => Icons.Material.Filled.Code;
    public string Color => string.Empty;
    public string Category => "Visual Studio";
    public string[] Tags => [".sln", ".slnx"];
    
    public async Task OnClick()
    {
        var model = new ChatConfiguration()
        {
            Id = Guid.NewGuid().ToString(),
            AssistantModeId = DevelopOptions.AssistantModeId,
        };
        model[DevelopOptions.BuilderId] = $"visual.studio.solution";
        model[ChatConfiguration.Icon] = Icon;
        model[ChatConfiguration.Group] = Category;
        model.HistoryInclusion = HistoryInclusionMode.Instruct;
        var parameters = new DialogParameters<Configure>
        {
            { nameof(Configure.Model), model },
            {nameof(Configure.Filter), new[]{"sln", "slnx"}},
            {nameof(Configure.IsSolutionFile), true}
        };
        var options = new DialogOptions { MaxWidth = MaxWidth.Small, FullWidth = true };
        var dialog1 = await dialog.ShowAsync<Configure>(Title, parameters, options);
        var result = await dialog1.Result;
        if (result is { Canceled: false })
        {
            await kernel.Create(model);
            nav.NavigateTo($"converse/{model.Id}");
        }
    }
}