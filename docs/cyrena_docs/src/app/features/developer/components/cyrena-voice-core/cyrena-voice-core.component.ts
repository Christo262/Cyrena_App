import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-cyrena-voice-core',
  standalone: true,
  imports: [],
  templateUrl: './cyrena-voice-core.component.html',
  styleUrl: './cyrena-voice-core.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CyrenaVoiceCoreComponent {
  readonly iaudioPlayerCode = `public interface IAudioPlayer : IDisposable
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
    Task DeinitializeAsync(CancellationToken cancellationToken = default);
    bool IsInitialized { get; }
    bool IsPlaying { get; }
    Task PlayAsync(AudioArtifact audio, CancellationToken cancellationToken = default);
}`;

  readonly itextAudioConverterCode = `public interface ITextAudioConverter : IDisposable
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
    Task DeinitializeAsync(CancellationToken cancellationToken = default);
    bool IsInitialized { get; }
    Task<AudioArtifact> ConvertAsync(string text, CancellationToken cancellationToken = default);
}`;

  readonly ivoiceChainCode = `public interface IVoiceChain
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
}`;

  readonly ivoiceRecorderCode = `public interface IVoiceRecorder : IDisposable
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
    Task DeinitializeAsync(CancellationToken cancellationToken = default);
    bool IsRecording { get; }
    bool IsInitialized { get; }
    Task StartRecording(CancellationToken cancellationToken = default);
    Task<VoiceArtifact> StopRecording(CancellationToken cancellationToken = default);
}`;

  readonly ivoiceTranscriberCode = `public interface IVoiceTranscriber : IDisposable
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
    Task DeinitializeAsync(CancellationToken cancellationToken = default);
    bool IsInitialized { get; }
    Task<string?> TranscribeAsync(VoiceArtifact artifact, CancellationToken cancellationToken = default);
}`;

  readonly audioArtifactCode = `public abstract class AudioArtifact
{
}`;

  readonly voiceArtifactCode = `public abstract class VoiceArtifact
{
}`;

  readonly emptyVoiceArtifactCode = `public sealed class EmptyVoiceArtifact : VoiceArtifact
{
}`;

  readonly standardWavVoiceArtifactCode = `public sealed class StandardWavVoiceArtifact : VoiceArtifact
{
    public StandardWavVoiceArtifact() { }
    public StandardWavVoiceArtifact(byte[] data)
    {
        Data = data;
    }

    public byte[] Data { get; init; } = [];
}`;

  readonly lifecycleCode = `[Created] -> InitializeAsync() -> [Operational] -> DeinitializeAsync() -> [Disposed]
                              |
                        Dispose()`;

  readonly pipelineFlowCode = `IVoiceRecorder    ->    IVoiceTranscriber    ->    ITextAudioConverter    ->    IAudioPlayer
     |                         |                           |                          |
StartRecording          TranscribeAsync             ConvertAsync                 PlayAsync
StopRecording           (returns text)            (returns AudioArtifact)      (plays audio)
(VoiceArtifact)`;

  readonly mp3AudioArtifactCode = `public sealed class Mp3AudioArtifact : AudioArtifact
{
    public byte[] Data { get; init; } = [];
    public int SampleRate { get; init; }
}`;
}
