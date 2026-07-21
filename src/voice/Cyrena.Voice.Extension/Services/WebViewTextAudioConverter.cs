using Cyrena.Contracts;
using Cyrena.Voice.Contracts;
using Cyrena.Voice.Models;
using Cyrena.Voice.Options;
using Microsoft.JSInterop;

namespace Cyrena.Voice.Services
{
    internal class WebViewTextAudioConverter : ITextAudioConverter
    {
        public const string Key = "webview.tts";
        private readonly IJSRuntime _js;
        private readonly ISettingsService _settings;
        public WebViewTextAudioConverter(IJSRuntime js, ISettingsService settings)
        {
            _js = js;
            _settings = settings;
        }

        public bool IsInitialized { get; private set;  }

        public async Task<AudioArtifact> ConvertAsync(string text, CancellationToken cancellationToken = default)
        {
            if(!string.IsNullOrEmpty(text))
            {
                var options = _settings.Read<VoiceOptions>(VoiceOptions.Key) ?? new VoiceOptions();

                if(string.IsNullOrEmpty(options.WebViewVoice))
                    await _js.InvokeVoidAsync("tts.speak", text, options.Rate, options.Pitch, options.Volume, cancellationToken);
                else
                    await _js.InvokeVoidAsync("tts.speakWithVoice", text, options.WebViewVoice, options.Rate, options.Pitch, options.Volume, cancellationToken);
            }
            return new WebViewAudioArtifact();
        }

        public async Task DeinitializeAsync(CancellationToken cancellationToken = default)
        {
            if (IsInitialized)
            {
                await _js.InvokeVoidAsync("tts.stop", cancellationToken);
                IsInitialized = false;
            }
        }

        public void Dispose()
        {
            IsInitialized = false;
        }

        public Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            IsInitialized = true;
            return Task.CompletedTask;
        }
    }
}
