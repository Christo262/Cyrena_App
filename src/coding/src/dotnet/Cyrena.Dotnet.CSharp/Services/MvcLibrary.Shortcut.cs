using Cyrena.Coding.Options;
using Cyrena.Contracts;
using Cyrena.Dotnet.CSharp.Components.Shared;
using Cyrena.Dotnet.Options;
using Cyrena.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Cyrena.Dotnet.CSharp.Services
{
    internal class MvcLibraryShortcut : IShortcut
    {
        private readonly IDialogService _dialog;
        private readonly IKernelController _kernel;
        private readonly NavigationManager _nav;
        public MvcLibraryShortcut(IDialogService dialog, IKernelController kernel, NavigationManager nav)
        {
            _dialog = dialog;
            _kernel = kernel;
            _nav = nav;
        }

        public string Title => MvcLibrary.Name;
        public string Description => "Develop a Model-View-Controller shared library.";
        public string Icon => "bi bi-collection";
        public string Color => "secondary";
        public string Category => ".NET Development";
        public string[] Tags => ["C#", "csproj"];

        public async Task OnClick()
        {
            var model = new ChatConfiguration()
            {
                Id = Guid.NewGuid().ToString(),
                AssistantModeId = DevelopOptions.AssistantModeId,
            };
            model[DevelopOptions.BuilderId] = MvcLibrary.Id;
            model[ChatConfiguration.Icon] = Icon;
            model[ChatConfiguration.Group] = ".NET Development";
            model.HistoryInclusion = HistoryInclusionMode.Instruct;
            var parameters = new DialogParameters<DotnetCsConfig>
            {
                { nameof(DotnetCsConfig.Model), model }
            };
            var options = new DialogOptions { MaxWidth = MaxWidth.Small, FullWidth = true };
            var dialog = await _dialog.ShowAsync<DotnetCsConfig>(Title, parameters, options);
            var result = await dialog.Result;
            if (result is { Canceled: false })
            {
                await _kernel.Create(model);
                _nav.NavigateTo($"converse/{model.Id}");
            }
        }
    }
}