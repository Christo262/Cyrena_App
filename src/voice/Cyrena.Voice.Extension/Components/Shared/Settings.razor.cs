using Cyrena.Contracts;
using Cyrena.Voice.Contracts;
using Cyrena.Voice.Options;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using MudBlazor;
using System.Text.Json.Serialization;

namespace Cyrena.Voice.Components.Shared
{
    public partial class Settings
    {
        [Inject] private IServiceProvider _services { get; set; } = null!;
        [Inject] private ISettingsService _settings { get; set; } = null!;
        [Inject] private IFileDialog _file { get; set; } = null!;
        [Inject] private IJSRuntime _js { get; set; } = null!;
        [Inject] private ISnackbar _snackbar { get; set; } = null!;

        private VoiceOptions _options = null!;
        private ESpeakOptions _espeak = null!;
        
        private IEnumerable<IVoiceChain> _chains { get; set; } = [];

        protected override void OnInitialized()
        {
            _options = _settings.Read<VoiceOptions>(VoiceOptions.Key) ?? new VoiceOptions();
            _espeak = _settings.Read<ESpeakOptions>(ESpeakOptions.Key) ?? new ESpeakOptions();
            _chains = _services.GetServices<IVoiceChain>();
            _r = (int)(_options.Rate * 10);
            _p = (int)(_options.Pitch * 10);
            _v = (int)(_options.Volume * 10);
        }

        private List<WebViewVoice> _voices { get; set; } = [];
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender) return;
            try
            {
                _voices = await _js.InvokeAsync<List<WebViewVoice>>("window.tts.getVoices");
                if (_voices.Count == 0)
                {
                    await Task.Delay(50);
                    _voices = await _js.InvokeAsync<List<WebViewVoice>>("window.tts.getVoices");
                }
                this.StateHasChanged();
            }
            catch (Exception ex)
            {
                _snackbar.Add(ex.Message, Severity.Error);
            }
        }

        private void Save()
        {
            _options.Rate = (float)_r / 10.0f;
            _options.Pitch = (float)_p / 10.0f;
            _options.Volume = (float)_v / 10.0f;
            _settings.Save(VoiceOptions.Key, _options);
            _settings.Save(ESpeakOptions.Key, _espeak);
        }

        private async Task SelectGgml()
        {
            var path = await _file.OpenAsync("Select GGML File", ("bin", [".bin"]));
            if (!string.IsNullOrEmpty(path))
            {
                _options.WhisperModelPath = path;
                Save();
            }
        }

        private int _r { get; set; }
        private int _p { get; set; }
        private int _v { get; set; }
    }

    internal class WebViewVoice
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }
        [JsonPropertyName("lang")]
        public string? Lang { get; set; }
    }
}
