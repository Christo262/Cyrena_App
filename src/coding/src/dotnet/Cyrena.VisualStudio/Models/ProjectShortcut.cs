using Cyrena.Coding.Options;
using Cyrena.Contracts;
using Cyrena.Models;
using Cyrena.VisualStudio.Components.Shared;
using Cyrena.VisualStudio.Contracts;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Cyrena.VisualStudio.Models;

public class ProjectShortcut : IShortcut
{
    private readonly IDialogService _dialog;
    private readonly IProjHandler _handler;
    private readonly IKernelController _kernel;
    private readonly NavigationManager _nav;
    public ProjectShortcut(IDialogService dialog, IKernelController kernel, NavigationManager nav,  IProjHandler handler)
    {
        _dialog = dialog;
        _kernel = kernel;
        _nav = nav;
        _handler = handler;
    }
    
    public async Task OnClick()
    {
        var model = new ChatConfiguration()
        {
            Id = Guid.NewGuid().ToString(),
            AssistantModeId = DevelopOptions.AssistantModeId,
        };
        model[DevelopOptions.BuilderId] = $"visual.studio.{_handler.Filter}";
        model[ChatConfiguration.Icon] = Icon;
        model[ChatConfiguration.Group] = Category;
        model.HistoryInclusion = HistoryInclusionMode.Instruct;
        var parameters = new DialogParameters<Configure>
        {
            { nameof(Configure.Model), model },
            {nameof(Configure.Filter), _handler.Filter}
        };
        var options = new DialogOptions { MaxWidth = MaxWidth.Small, FullWidth = true };
        var dialog = await _dialog.ShowAsync<Configure>(Title, parameters, options);
        var result = await dialog.Result;
        if (result is { Canceled: false })
        {
            await _kernel.Create(model);
            _nav.NavigateTo($"converse/{model.Id}");
        }
    }

    public string Title => _handler.Title;
    public string Description => _handler.Description;
    public string Icon => Icons.Material.Filled.Code;
    public string Color => string.Empty;
    public string Category => "Visual Studio";
    public string[] Tags => [_handler.Filter];
}