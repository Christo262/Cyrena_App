using BootstrapBlazor.Components;
using Cyrena.Coding.Options;
using Cyrena.Contracts;
using Cyrena.Dotnet.Components.Shared;
using Cyrena.Dotnet.Options;
using Cyrena.Models;
using Microsoft.AspNetCore.Components;
namespace Cyrena.Dotnet.Models
{
    internal class DotnetShortcut : IShortcut
    {
        private readonly DialogService _dialog;
        private readonly IKernelController _kernel;
        private readonly NavigationManager _nav;
        public DotnetShortcut(DialogService dialog, IKernelController kernel, NavigationManager nav)
        {
            _dialog = dialog;
            _kernel = kernel;
            _nav = nav;
        }

        public string Title => ".NET Solution";
        public string Description => "Develop a new .NET solution.";

        public string Icon => "bi bi-webcam";
        public string Color => "info";
        public string Category => ".NET";
        public string[] Tags => ["sln", "slnx"];

        public async Task OnClick()
        {
            var model = new ChatConfiguration()
            {
                Id = Guid.NewGuid().ToString(),
                AssistantModeId = DevelopOptions.AssistantModeId,
            };
            model[DevelopOptions.BuilderId] = ".net-solution";
            model[ChatConfiguration.Icon] = Icon;
            model[ChatConfiguration.Group] = ".NET Development";
            var rf = await _dialog.ShowModal<DotnetConversationForm>(new ResultDialogOption()
            {
                Title = ".NET Solution",
                Size = Size.Medium,
                ComponentParameters = new()
                {
                    {nameof(DotnetConversationForm.Configuration), model }
                },
                ButtonNoText = "Cancel",
                ButtonYesText = "Submit"
            });
            if(rf == DialogResult.Yes)
            {
                await _kernel.Create(model);
                _nav.NavigateTo($"converse/{model.Id}");
            }
        }
    }
}
