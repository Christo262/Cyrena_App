using BootstrapBlazor.Components;
using Cyrena.Coding.Options;
using Cyrena.Contracts;
using Cyrena.Dotnet.CSharp.Components.Shared;
using Cyrena.Models;
using Microsoft.AspNetCore.Components;

namespace Cyrena.Dotnet.CSharp.Services
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
