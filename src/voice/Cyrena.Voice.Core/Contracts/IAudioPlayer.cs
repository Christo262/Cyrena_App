using Cyrena.Voice.Models;

namespace Cyrena.Voice.Contracts
{
    /// <summary>
    /// Plays AudioArtifact
    /// </summary>
    public interface IAudioPlayer : IDisposable
    {
        Task InitializeAsync(CancellationToken cancellationToken = default);
        Task DeinitializeAsync(CancellationToken cancellationToken = default);
        bool IsInitialized { get; }
        bool IsPlaying { get; }
        Task PlayAsync(AudioArtifact audio, CancellationToken cancellationToken = default);
    }
}
