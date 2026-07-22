using Cyrena.Coding.Options;
using Cyrena.Contracts;
using Cyrena.Dotnet.CSharp.Components.Shared;
using Cyrena.Dotnet.Options;
using Cyrena.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Cyrena.Dotnet.CSharp.Models
{
    internal class DotnetShortcut : IShortcut
    {
        private readonly IDialogService _dialog;
        private readonly IKernelController _kernel;
        private readonly NavigationManager _nav;

        public DotnetShortcut(IDialogService dialog, IKernelController kernel, NavigationManager nav)
        {
            _dialog = dialog;
            _kernel = kernel;
            _nav = nav;
        }

        public string Title => ".NET Solution";
        public string Description => "Develop a new .NET solution.";
        public string Icon => "bi bi-webcam";
        public string Color => "info";
        public string Category => ".NET C# Development";
        public string[] Tags => ["sln", "slnx"];

        public async Task OnClick()
        {
            var model = new ChatConfiguration
            {
                Id = Guid.NewGuid().ToString(),
                AssistantModeId = DevelopOptions.AssistantModeId,
            };
            model[DevelopOptions.BuilderId] = ".net-solution";
            model[ChatConfiguration.Icon] = Icon;
            model[ChatConfiguration.Group] = Category;
            model.HistoryInclusion = HistoryInclusionMode.Instruct;

            var parameters = new DialogParameters<DotnetConversationForm>
            {
                { nameof(DotnetConversationForm.Configuration), model }
            };
            var options = new DialogOptions { MaxWidth = MaxWidth.Small, FullWidth = true };
            var dialog = await _dialog.ShowAsync<DotnetConversationForm>(".NET Solution", parameters, options);
            var result = await dialog.Result;
            if (result is { Canceled: false })
            {
                await _kernel.Create(model);
                _nav.NavigateTo($"converse/{model.Id}");
            }
        }
    }
}
