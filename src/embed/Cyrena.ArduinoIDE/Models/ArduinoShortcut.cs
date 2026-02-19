using BootstrapBlazor.Components;
using Cyrena.ArduinoIDE.Components.Shared;
using Cyrena.ArduinoIDE.Options;
using Cyrena.Contracts;
using Cyrena.Developer.Options;
using Cyrena.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cyrena.ArduinoIDE.Models
{
    internal class ArduinoShortcut : IShortcut
    {
        private readonly DialogService _dialog;
        private readonly IKernelController _kernel;
        private readonly NavigationManager _nav;
        public ArduinoShortcut(DialogService dialog, IKernelController kernel, NavigationManager nav)
        {
            _dialog = dialog;
            _kernel = kernel;
            _nav = nav;
        }

        public string Title => "Arduino IDE";
        public string Description => "Develop a Arduino IDE sketch";
        public string Icon => "bi bi-cpu";
        public string Color => "primary";

        public async Task OnClick()
        {
            var model = new ChatConfiguration()
            {
                Id = Guid.NewGuid().ToString(),
                AssistantModeId = DevelopOptions.AssistantModeId,
            };
            model[DevelopOptions.BuilderId] = ArduinoOptions.BuilderId;
            model[ChatConfiguration.Icon] = Icon;
            model[ChatConfiguration.Group] = "Embedded";
            var rf = await _dialog.ShowModal<Configure>(new ResultDialogOption()
            {
                Title = "Arduino IDE",
                Size = Size.Medium,
                ComponentParameters = new()
                {
                    {nameof(Configure.Model), model }
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
