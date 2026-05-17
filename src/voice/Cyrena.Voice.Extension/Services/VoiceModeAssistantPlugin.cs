using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Models;
using Cyrena.Voice.Components.Shared;
using Cyrena.Voice.Options;

namespace Cyrena.Voice.Services
{
    internal class VoiceModeAssistantPlugin : IAssistantPlugin
    {
        private readonly ISettingsService _settings;
        public VoiceModeAssistantPlugin(ISettingsService settings)
        {
            _settings = settings;
        }

        public string Id => "cyrena.voice";
        public string[] Modes => [];
        public int Priority => 20;
        public bool Required => false;
        public string Title => "Voice Mode";

        public Task LoadAsync(CyrenaKernelBuilder builder)
        {
            builder.AddToolbarComponent<Toolbar>(ToolbarAlignment.End);
            var options = _settings.Read<WebViewVoiceOptions>(WebViewVoiceOptions.Key) ?? new WebViewVoiceOptions();
            if(options.DefaultVoiceChain == WebViewWhisperVoiceChain.Key)
            {
                if (string.IsNullOrEmpty(options.WhisperModelPath))
                    throw new InvalidOperationException("Please configure path to Whisper model in Settings");
                if (!File.Exists(options.WhisperModelPath))
                    throw new FileNotFoundException($"Unable to find Whisper model at {options.WhisperModelPath}");
            }
            return Task.CompletedTask;
        }
    }
}
