using Cyrena.Voice.Models;

namespace Cyrena.Voice.Contracts
{
    /// <summary>
    /// Records voice input
    /// </summary>
    public interface IVoiceRecorder : IDisposable
    {
        Task InitializeAsync(CancellationToken cancellationToken = default);
        Task DeinitializeAsync(CancellationToken cancellationToken = default);
        bool IsRecording { get; }
        bool IsInitialized { get; }
        Task StartRecording(CancellationToken cancellationToken = default);
        Task<VoiceArtifact> StopRecording(CancellationToken cancellationToken = default);
    }
}
