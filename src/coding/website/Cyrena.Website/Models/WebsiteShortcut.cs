using BootstrapBlazor.Components;
using Cyrena.Coding.Options;
using Cyrena.Components;
using Cyrena.Contracts;
using Cyrena.Models;
using Cyrena.Website.Components.Shared;
using Cyrena.Website.Options;
using Microsoft.AspNetCore.Components;

namespace Cyrena.Website.Models
{
    internal class WebsiteShortcut : IShortcut
    {
        private readonly IKernelController _kernel;
        private readonly NavigationManager _nav;
        private readonly DialogService _dialog;

        public WebsiteShortcut(IKernelController kernel, NavigationManager nav, DialogService dialog)
        {
            _kernel = kernel;
            _nav = nav;
            _dialog = dialog;
        }

        public string Title => "Website";
        public string Description => "Develop a static website";
        public string Icon => "bi bi-globe";
        public string Color => "primary";
        public string Category => "Web Development";
        public string[] Tags => ["HTML", "CSS", "JavaScript", "Web", "Frontend"];

        public async Task OnClick()
        {
            var model = new ChatConfiguration
            {
                Id = Guid.NewGuid().ToString(),
                AssistantModeId = DevelopOptions.AssistantModeId,
            };
            model[DevelopOptions.BuilderId] = WebsiteOptions.BuilderId;
            model[ChatConfiguration.Icon] = Icon;
            model[ChatConfiguration.Group] = Category;

            var result = await _dialog.ShowModal<Configure>(new ResultDialogOption()
            {
                Title = "Website",
                Size = Size.Medium,
                ComponentParameters = new()
                {
                    { nameof(Configure.Model), model }
                },
                ButtonYesText = "Create",
                ButtonNoText = "Cancel",
            });

            if (result != DialogResult.Yes)
                return;

            await _kernel.Create(model);
            _nav.NavigateTo($"converse/{model.Id}");
        }
    }
}
