using Cyrena.Voice.Models;

namespace Cyrena.Voice.Contracts
{
    /// <summary>
    /// Converts voice input into text
    /// </summary>
    public interface IVoiceTranscriber : IDisposable
    {
        Task InitializeAsync(CancellationToken cancellationToken = default);
        Task DeinitializeAsync(CancellationToken cancellationToken = default);
        bool IsInitialized { get; }
        Task<string?> TranscribeAsync(VoiceArtifact artifact, CancellationToken cancellationToken = default);
    }
}
