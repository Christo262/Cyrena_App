using BootstrapBlazor.Components;
using Cyrena.Contracts;
using Cyrena.Developer.Options;
using Cyrena.Models;
using Cyrena.PlatformIO.Components.Shared;
using Cyrena.PlatformIO.Options;
using Microsoft.AspNetCore.Components;

namespace Cyrena.PlatformIO.Models
{
    internal class PlatformIOShortcut : IShortcut
    {
        private readonly DialogService _dialog;
        private readonly IKernelController _kernel;
        private readonly NavigationManager _nav;
        public PlatformIOShortcut(DialogService dialog, IKernelController kernel, NavigationManager nav)
        {
            _dialog = dialog;
            _kernel = kernel;
            _nav = nav;
        }
        public string Title => "PlatformIO";

        public string Description => "Develop firmware with PlatformIO";

        public string Icon => "bi bi-cpu";

        public string Color => "danger";

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
            var rf = await _dialog.ShowModal<Configure>(new ResultDialogOption()
            {
                Title = "PlatformIO",
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
