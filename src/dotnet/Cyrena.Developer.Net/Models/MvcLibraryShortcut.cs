using BootstrapBlazor.Components;
using Cyrena.Contracts;
using Cyrena.Developer.Components.Shared;
using Cyrena.Developer.Options;
using Cyrena.Models;
using Microsoft.AspNetCore.Components;

namespace Cyrena.Developer.Models
{
    internal class MvcLibraryShortcut : IShortcut
    {
        private readonly DialogService _dialog;
        private readonly IKernelController _kernel;
        private readonly NavigationManager _nav;
        public MvcLibraryShortcut(DialogService dialog, IKernelController kernel, NavigationManager nav)
        {
            _dialog = dialog;
            _kernel = kernel;
            _nav = nav;
        }

        public string Title => ".NET MVC Library";
        public string Description => "Develop a Model-View-Controller shared library.";
        public string Icon => "bi bi-collection";
        public string Color => "secondary";

        public async Task OnClick()
        {
            var model = new ChatConfiguration()
            {
                Id = Guid.NewGuid().ToString(),
                AssistantModeId = DevelopOptions.AssistantModeId,
            };
            model[DevelopOptions.BuilderId] = DotnetOptions.CsMvcLib;
            model[ChatConfiguration.Icon] = Icon;
            model[ChatConfiguration.Group] = ".NET Development";
            var rf = await _dialog.ShowModal<DotnetCsConfig>(new ResultDialogOption()
            {
                Title = Title,
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
