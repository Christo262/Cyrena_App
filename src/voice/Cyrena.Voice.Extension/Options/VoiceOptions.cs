namespace Cyrena.Voice.Options
{
    public class VoiceOptions
    {
        public const string Key = "webview.voice";

        public string DefaultVoiceChain { get; set; } = Cyrena.Voice.Services.WebViewWhisperVoiceChain.Key;
        public string? WhisperModelPath { get; set; }
        public string? WebViewVoice { get; set; }

        //0.5 - 2.0
        public float Rate { get; set; } = 1.0f;
        //0.5 - 1.5
        public float Pitch { get; set; } = 1.0f;
        //0 - 1
        public float Volume { get; set;  } = 1.0f;
    }
}
