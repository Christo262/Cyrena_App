using BootstrapBlazor.Components;
using Cyrena.Contracts;
using Cyrena.Dotnet.Components.Shared;
using Cyrena.Dotnet.Options;
using Cyrena.Models;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cyrena.Dotnet.Models
{
    internal class BlazorLibraryShortcut : IShortcut
    {
        private readonly DialogService _dialog;
        private readonly IKernelController _kernel;
        private readonly NavigationManager _nav;
        public BlazorLibraryShortcut(DialogService dialog, IKernelController kernel, NavigationManager nav)
        {
            _dialog = dialog;
            _kernel = kernel;
            _nav = nav;
        }

        public string Title => "Blazor Library";
        public string Description => "Develop a Blazor component library.";
        public string Icon => "bi bi-hdd-rack";
        public string Color => "danger";
        public string Category => ".NET";
        public string[] Tags => ["C#", "csproj"];

        public async Task OnClick()
        {
            var model = new ChatConfiguration()
            {
                Id = Guid.NewGuid().ToString(),
                AssistantModeId = DevelopOptions.AssistantModeId,
            };
            model[DevelopOptions.BuilderId] = DotnetOptions.CsBlazorLibrary;
            model[ChatConfiguration.Icon] = Icon;
            model[ChatConfiguration.Group] = ".NET Development";
            var rf = await _dialog.ShowModal<DotnetCsConfig>(new ResultDialogOption()
            {
                Title = "Blazor Library",
                Size = Size.Medium,
                ComponentParameters = new()
                {
                    {nameof(DotnetCsConfig.Model), model }
                },
                ButtonNoText = "Cancel",
                ButtonYesText = "Submit"
            });
            if (rf == DialogResult.Yes)
            {
                await _kernel.Create(model);
                _nav.NavigateTo($"converse/{model.Id}");
            }
        }
    }
}
