using BootstrapBlazor.Components;
using Cyrena.Contracts;
using Cyrena.Developer.Components.Shared;
using Cyrena.Developer.Options;
using Cyrena.Models;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cyrena.Developer.Models
{
    internal class BlazorAppShortuct : IShortcut
    {
        private readonly DialogService _dialog;
        private readonly IKernelController _kernel;
        private readonly NavigationManager _nav;
        public BlazorAppShortuct(DialogService dialog, IKernelController kernel, NavigationManager nav)
        {
            _dialog = dialog;
            _kernel = kernel;
            _nav = nav;
        }

        public string Title => "Blazor App";
        public string Description => "Develop a Blazor Server or WASM app.";
        public string Icon => "bi bi-hdd-rack";
        public string Color => "danger";

        public async Task OnClick()
        {
            var model = new ChatConfiguration()
            {
                Id = Guid.NewGuid().ToString(),
                AssistantModeId = DevelopOptions.AssistantModeId,
            };
            model[DevelopOptions.BuilderId] = DotnetOptions.CsBlazorApp;
            model[ChatConfiguration.Icon] = Icon;
            model[ChatConfiguration.Group] = ".NET Development";
            var rf = await _dialog.ShowModal<DotnetCsConfig>(new ResultDialogOption()
            {
                Title = "Blazor App",
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
