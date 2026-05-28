using Cyrena.Contracts;
using Cyrena.Ollama.Web.Options;
using Microsoft.AspNetCore.Components;

namespace Cyrena.Ollama.Web.Components.Shared
{
    public partial class Settings
    {
        [Inject] private ISettingsService _settings { get; set; } = default!;

        private OllamaWebOptions _model { get; set; } = default!;

        protected override void OnInitialized()
        {
            _model = _settings.Read<OllamaWebOptions>(OllamaWebOptions.Key) ?? new OllamaWebOptions();
        }

        private void Save()
        {
            _settings.Save(OllamaWebOptions.Key, _model);
        }

        private void SetApiKey(string? key)
        {
            _model.APIKey = key;
            Save();
        }

        private void SetEnabled(bool e)
        {
            _model.Enabled = e;
            Save();
        }
    }
}
