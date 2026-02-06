using BootstrapBlazor.Components;
using Cyrena.Contracts;
using Cyrena.Runtime.OpenAI.Options;
using Microsoft.AspNetCore.Components;

namespace Cyrena.Runtime.OpenAI.Components.Shared
{
    public partial class OpenAISettings
    {
        [Inject] private ISettingsService _settings { get; set; } = default!;
        [Inject] private ToastService _toasts { get;set;  } = default!;

        private OpenAIOptions? _model;

        public override void OnFirstRender()
        {
            _model = _settings.Read<OpenAIOptions>(OpenAIOptions.Key) ?? new OpenAIOptions();
            this.StateHasChanged();
        }

        private async Task Save()
        {
            if(_model == null) return;
            _settings.Save(OpenAIOptions.Key, _model);
            await _toasts.Success("OpenAI Settings", "OpenAI settings saved");
        }
    }
}
