using BootstrapBlazor.Components;
using Cyrena.Contracts;
using Cyrena.Voice.Contracts;
using Cyrena.Voice.Options;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using PuppeteerSharp;
using System.Text.Json.Serialization;

namespace Cyrena.Voice.Components.Shared
{
    public partial class Settings
    {
        [Inject] private IServiceProvider _services { get; set; } = default!;
        [Inject] private ISettingsService _settings { get; set; } = default!;
        [Inject] private IFileDialog _file { get; set; } = default!;
        [Inject] private IJSRuntime _js { get; set; } = default!;
        [Inject] private ToastService _toasts { get; set; } = default!;

        private WebViewVoiceOptions _options = default!;
        private IEnumerable<IVoiceChain> _chains { get; set; } = [];
        private List<SelectedItem> _items { get; set; } = [];

        protected override void OnInitialized()
        {
            _options = _settings.Read<WebViewVoiceOptions>(WebViewVoiceOptions.Key) ?? new WebViewVoiceOptions();
            _chains = _services.GetServices<IVoiceChain>();
            _items = _chains.Select(x => new SelectedItem()
            {
                Value = x.Id,
                Text = x.Name,
                Active = _options.DefaultVoiceChain == x.Id
            }).ToList();
            _items.Insert(0, new SelectedItem()
            {
                Text = "Disabled",
                Active = string.IsNullOrEmpty(_options.DefaultVoiceChain)
            });
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
                if(_voices.Count == 0)
                {
                    await Task.Delay(50);
                    _voices = await _js.InvokeAsync<List<WebViewVoice>>("window.tts.getVoices");
                }
                this.StateHasChanged();
            }catch (Exception ex)
            {
                await _toasts.Error("Error", ex.Message);
            }
        }

        private void Save()
        {
            _options.Rate = (float)_r / 10.0f;
            _options.Pitch = (float)_p / 10.0f;
            _options.Volume = (float)_v / 10.0f;
            _settings.Save(WebViewVoiceOptions.Key, _options);
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
