using BootstrapBlazor.Components;
using Cyrena.Coding.Options;
using Cyrena.Contracts;
using Cyrena.Dotnet.CSharp.Components.Shared;
using Cyrena.Models;
using Microsoft.AspNetCore.Components;

namespace Cyrena.Dotnet.CSharp.Services
{
    internal class ConsoleAppShortcut : IShortcut
    {
        private readonly DialogService _dialog;
        private readonly IKernelController _kernel;
        private readonly NavigationManager _nav;

        public ConsoleAppShortcut(DialogService dialog, IKernelController kernel, NavigationManager nav)
        {
            _dialog = dialog;
            _kernel = kernel;
            _nav = nav;
        }

        public string Title => ConsoleApplication.Name;
        public string Description => "Develop a .NET C# Console Application.";
        public string Icon => "bi bi-terminal";
        public string Color => "primary";
        public string Category => ".NET Development";
        public string[] Tags => ["C#", "csproj"];

        public async Task OnClick()
        {
            var model = new ChatConfiguration()
            {
                Id = Guid.NewGuid().ToString(),
                AssistantModeId = DevelopOptions.AssistantModeId,
            };
            model[DevelopOptions.BuilderId] = ConsoleApplication.Id;
            model[ChatConfiguration.Icon] = Icon;
            model[ChatConfiguration.Group] = ".NET Development";
            var rf = await _dialog.ShowModal<DotnetCsConfig>(new ResultDialogOption()
            {
                Title = "Console App",
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
