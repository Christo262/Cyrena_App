using Cyrena.Angular.Components.Shared;
using Cyrena.Angular.Options;
using Cyrena.Contracts;
using Cyrena.Coding.Options;
using Cyrena.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Cyrena.Angular.Models
{
    internal class AngularShortcut : IShortcut
    {
        private readonly IDialogService _dialog;
        private readonly IKernelController _kernel;
        private readonly NavigationManager _nav;

        public AngularShortcut(IDialogService dialog, IKernelController kernel, NavigationManager nav)
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
            model.HistoryInclusion = HistoryInclusionMode.Instruct;

            var parameters = new DialogParameters<Configure>
            {
                { x => x.Model, model }
            };
            var options = new DialogOptions { MaxWidth = MaxWidth.Small };
            var dialog = await _dialog.ShowAsync<Configure>("Angular", parameters, options);
            var result = await dialog.Result;

            if (result is not null && !result.Canceled)
            {
                await _kernel.Create(model);
                _nav.NavigateTo($"converse/{model.Id}");
            }
        }
    }
}