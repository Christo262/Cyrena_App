using Cyrena.Voice.Models;

namespace Cyrena.Voice.Contracts
{
    /// <summary>
    /// Converts text to AudioArtiface
    /// </summary>
    public interface ITextAudioConverter : IDisposable
    {
        Task InitializeAsync(CancellationToken cancellationToken = default);
        Task DeinitializeAsync(CancellationToken cancellationToken = default);
        bool IsInitialized { get; }
        Task<AudioArtifact> ConvertAsync(string text, CancellationToken cancellationToken = default);
    }
}
