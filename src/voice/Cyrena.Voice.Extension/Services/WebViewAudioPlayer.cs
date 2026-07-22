using Cyrena.Voice.Contracts;
using Cyrena.Voice.Models;

namespace Cyrena.Voice.Services
{
    internal class WebViewAudioPlayer : IAudioPlayer
    {
        public const string Key = "no.audio";
        public bool IsInitialized { get; private set; }

        public bool IsPlaying { get; private set; }

        public Task DeinitializeAsync(CancellationToken cancellationToken = default)
        {
            IsInitialized = false;
            return Task.CompletedTask;
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

        public Task PlayAsync(AudioArtifact audio, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
