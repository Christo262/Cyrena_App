using Cyrena.Contracts;
using Cyrena.Voice.Contracts;
using Cyrena.Voice.Models;
using Microsoft.JSInterop;

namespace Cyrena.Voice.Services
{
    internal class WebViewVoiceRecorder : IVoiceRecorder
    {
        public const string Key = "webview.recorder";
        private readonly IJSRuntime _js;
        public WebViewVoiceRecorder(IJSRuntime js)
        {
            _js = js;
        }

        public bool IsRecording { get; private set; }
        public bool IsInitialized { get; private set; }

        public async Task DeinitializeAsync(CancellationToken cancellationToken = default)
        {
            if (!IsInitialized)
                return;
            if (IsRecording)
            {
                await _js.InvokeAsync<string>("audioRecorder.stopRecording");
                IsRecording = false;
            }
            IsInitialized = false;
        }

        public void Dispose()
        {
            _ = DeinitializeAsync();
        }

        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            if(!IsInitialized)
            {
                await _js.InvokeVoidAsync("window.tts.initialize");
                IsInitialized = true;
            }
        }

        public async Task StartRecording(CancellationToken cancellationToken = default)
        {
            if (IsRecording)
                return;
            await _js.InvokeVoidAsync("audioRecorder.startRecording");
            IsRecording = true;
        }

        public async Task<VoiceArtifact> StopRecording(CancellationToken cancellationToken = default)
        {
            if (!IsRecording)
                return new EmptyVoiceArtifact();
            var base64Audio = await _js.InvokeAsync<string>("audioRecorder.stopRecording");
            IsRecording = false;
            var audioBytes = Convert.FromBase64String(base64Audio.Split(',')[1]);
            return new StandardWavVoiceArtifact(audioBytes);
        }
    }
}
