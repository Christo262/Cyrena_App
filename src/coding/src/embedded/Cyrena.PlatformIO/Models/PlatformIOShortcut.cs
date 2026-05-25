using Cyrena.PlatformIO.Components.Shared;
using Cyrena.PlatformIO.Options;
using Cyrena.Contracts;
using Cyrena.Coding.Options;
using Cyrena.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System;
using System.Threading.Tasks;

namespace Cyrena.PlatformIO.Models
{
    internal class PlatformIOShortcut : IShortcut
    {
        private readonly IDialogService _dialog;
        private readonly IKernelController _kernel;
        private readonly NavigationManager _nav;
        public PlatformIOShortcut(IDialogService dialog, IKernelController kernel, NavigationManager nav)
        {
            _dialog = dialog;
            _kernel = kernel;
            _nav = nav;
        }

        public string Title => "PlatformIO";
        public string Description => "Develop a PlatformIO project";
        public string Icon => "bi bi-cpu";
        public string Color => "primary";
        public string Category => "Embedded";
        public string[] Tags => ["C++", "IoT", "Embedded"];

        public async Task OnClick()
        {
            var model = new ChatConfiguration()
            {
                Id = Guid.NewGuid().ToString(),
                AssistantModeId = DevelopOptions.AssistantModeId,
            };
            model[DevelopOptions.BuilderId] = PlatformIOOptions.BuilderId;
            model[ChatConfiguration.Icon] = Icon;
            model[ChatConfiguration.Group] = "Embedded";
            model.HistoryInclusion = HistoryInclusionMode.Instruct;

            var parameters = new DialogParameters<Configure>
            {
                { x => x.Model, model }
            };
            var options = new DialogOptions { MaxWidth = MaxWidth.Small };
            var dialog = await _dialog.ShowAsync<Configure>("PlatformIO", parameters, options);
            var result = await dialog.Result;

            if (result is not null && !result.Canceled)
            {
                await _kernel.Create(model);
                _nav.NavigateTo($"converse/{model.Id}");
            }
        }
    }
}