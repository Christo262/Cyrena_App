namespace Cyrena.Voice.Contracts
{
    /// <summary>
    /// Holds the flow together
    /// </summary>
    public interface IVoiceChain
    {
        string Id { get; }
        string Name { get; }
        string? Description { get; }

        Task InitializeAsync(CancellationToken cancellationToken = default);
        Task DeinitializeAsync(CancellationToken cancellationToken = default);

        bool IsInitialized { get; }

        IVoiceRecorder Recorder { get; }
        IVoiceTranscriber Transcriber { get; }
        ITextAudioConverter Converter { get; }
        IAudioPlayer Player { get; }
    }
}
