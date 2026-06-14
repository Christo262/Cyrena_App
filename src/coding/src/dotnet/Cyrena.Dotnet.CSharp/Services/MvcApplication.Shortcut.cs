using Cyrena.Coding.Options;
using Cyrena.Contracts;
using Cyrena.Dotnet.CSharp.Components.Shared;
using Cyrena.Dotnet.Options;
using Cyrena.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Cyrena.Dotnet.CSharp.Services
{
    internal class MvcAppShortcut : IShortcut
    {
        private readonly IDialogService _dialog;
        private readonly IKernelController _kernel;
        private readonly NavigationManager _nav;
        public MvcAppShortcut(IDialogService dialog, IKernelController kernel, NavigationManager nav)
        {
            _dialog = dialog;
            _kernel = kernel;
            _nav = nav;
        }

        public string Title => MvcApplication.Name;
        public string Description => "Develop a Model-View-Controller app.";
        public string Icon => "bi bi-menu-button-wide-fill";
        public string Color => "secondary";
        public string Category => ".NET C# Development";
        public string[] Tags => ["C#", "csproj"];

        public async Task OnClick()
        {
            var model = new ChatConfiguration()
            {
                Id = Guid.NewGuid().ToString(),
                AssistantModeId = DevelopOptions.AssistantModeId,
            };
            model[DevelopOptions.BuilderId] = MvcApplication.Id;
            model[ChatConfiguration.Icon] = Icon;
            model[ChatConfiguration.Group] = Category;
            model.HistoryInclusion = HistoryInclusionMode.Instruct;
            var parameters = new DialogParameters<DotnetCsConfig>
            {
                { nameof(DotnetCsConfig.Model), model }
            };
            var options = new DialogOptions { MaxWidth = MaxWidth.Small, FullWidth = true };
            var rf = await _dialog.ShowAsync<DotnetCsConfig>(".NET MVC", parameters, options);
            var result = await rf.Result;
            if (result is { Canceled: false })
            {
                await _kernel.Create(model);
                _nav.NavigateTo($"converse/{model.Id}");
            }
        }
    }
}