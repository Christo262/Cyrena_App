using BootstrapBlazor.Components;
using Cyrena.Angular.Components.Shared;
using Cyrena.Angular.Options;
using Cyrena.Contracts;
using Cyrena.Coding.Options;
using Cyrena.Models;
using Microsoft.AspNetCore.Components;

namespace Cyrena.Angular.Models
{
    internal class AngularShortcut : IShortcut
    {
        private readonly DialogService _dialog;
        private readonly IKernelController _kernel;
        private readonly NavigationManager _nav;

        public AngularShortcut(DialogService dialog, IKernelController kernel, NavigationManager nav)
        {
            _dialog = dialog;
            _kernel = kernel;
            _nav = nav;
        }

        public string Title => "Angular";
        public string Description => "Develop an Angular application";
        public string Icon => "bi bi-box";
        public string Color => "danger";
        public string Category => "Web Development";
        public string[] Tags => ["TypeScript", "Web", "Frontend"];

        public async Task OnClick()
        {
            var model = new ChatConfiguration()
            {
                Id = Guid.NewGuid().ToString(),
                AssistantModeId = DevelopOptions.AssistantModeId,
            };
            model[DevelopOptions.BuilderId] = AngularOptions.BuilderId;
            model[ChatConfiguration.Icon] = Icon;
            model[ChatConfiguration.Group] = Category;

            var rf = await _dialog.ShowModal<Configure>(new ResultDialogOption()
            {
                Title = "Angular",
                Size = Size.Medium,
                ComponentParameters = new()
                {
                    { nameof(Configure.Model), model }
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
