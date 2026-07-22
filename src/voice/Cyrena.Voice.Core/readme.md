## Cyrena.Voice.Core

**Project**: Cyrena.Voice.Core  
**Type**: Class Library  
**Target Framework**: .NET 10.0  
**Namespace**: `Cyrena.Voice`  
**Version**: 0.6.0

---

### Purpose

Cyrena.Voice.Core provides the foundational contracts and data models for Cyréna's voice processing pipeline. It defines the interfaces that voice implementations must satisfy and the artifact types that flow through the voice chain. This library contains no implementations — it is a pure contracts and models package intended to be referenced by voice provider extensions and the main application.

The voice pipeline follows a four-stage chain:
1. **Record** — `IVoiceRecorder` captures audio input
2. **Transcribe** — `IVoiceTranscriber` converts voice to text
3. **Convert** — `ITextAudioConverter` converts text back to audio (TTS)
4. **Play** — `IAudioPlayer` plays the resulting audio

These stages are orchestrated by `IVoiceChain`, which holds references to all four stages and manages their collective lifecycle.

---

### Project Structure

| Folder | Contents |
|--------|----------|
| **Contracts** | 5 interfaces defining the voice pipeline |
| **Models** | 4 data classes for voice/audio artifacts |
| **Attributes** | Empty (reserved for future use) |
| **Extensions** | Empty (reserved for future use) |
| **Options** | Empty (reserved for future use) |
| **Services** | Empty (no implementations in this package) |

---

### Contracts

#### IAudioPlayer

Plays `AudioArtifact` instances. Lifecycle-managed with explicit initialize/deinitialize pattern.

```csharp
namespace Cyrena.Voice.Contracts;

public interface IAudioPlayer : IDisposable
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
    Task DeinitializeAsync(CancellationToken cancellationToken = default);
    bool IsInitialized { get; }
    bool IsPlaying { get; }
    Task PlayAsync(AudioArtifact audio, CancellationToken cancellationToken = default);
}
```

**Behavior**:
- Must call `InitializeAsync` before `PlayAsync`. Behavior is undefined if `PlayAsync` is called before initialization.
- `IsPlaying` should reflect the actual playback state.
- `DeinitializeAsync` releases resources. `Dispose` should also clean up.
- All operations accept an optional `CancellationToken`.

---

#### ITextAudioConverter

Converts text to `AudioArtifact` (text-to-speech). Lifecycle-managed with explicit initialize/deinitialize pattern.

```csharp
namespace Cyrena.Voice.Contracts;

public interface ITextAudioConverter : IDisposable
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
    Task DeinitializeAsync(CancellationToken cancellationToken = default);
    bool IsInitialized { get; }
    Task<AudioArtifact> ConvertAsync(string text, CancellationToken cancellationToken = default);
}
```

**Behavior**:
- Must call `InitializeAsync` before `ConvertAsync`.
- Returns an `AudioArtifact` (typically a concrete subclass provided by the implementation).
- All operations accept an optional `CancellationToken`.

---

#### IVoiceChain

Holds the voice pipeline together. Provides access to all four stages and manages their collective lifecycle.

```csharp
namespace Cyrena.Voice.Contracts;

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
```

**Behavior**:
- `Id` is a unique identifier for the chain instance.
- `Name` is a display name.
- `Description` is an optional human-readable description.
- `InitializeAsync` should initialize all contained stages.
- `DeinitializeAsync` should deinitialize all contained stages.
- The four stage properties (`Recorder`, `Transcriber`, `Converter`, `Player`) expose the pipeline components.

---

#### IVoiceRecorder

Records voice input and produces `VoiceArtifact` instances. Lifecycle-managed with explicit initialize/deinitialize pattern.

```csharp
namespace Cyrena.Voice.Contracts;

public interface IVoiceRecorder : IDisposable
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
    Task DeinitializeAsync(CancellationToken cancellationToken = default);
    bool IsRecording { get; }
    bool IsInitialized { get; }
    Task StartRecording(CancellationToken cancellationToken = default);
    Task<VoiceArtifact> StopRecording(CancellationToken cancellationToken = default);
}
```

**Behavior**:
- Must call `InitializeAsync` before `StartRecording`.
- `StartRecording` begins capture. `IsRecording` should be `true` during capture.
- `StopRecording` ends capture and returns a `VoiceArtifact` containing the recorded audio.
- All operations accept an optional `CancellationToken`.

---

#### IVoiceTranscriber

Converts `VoiceArtifact` into text (speech-to-text). Lifecycle-managed with explicit initialize/deinitialize pattern.

```csharp
namespace Cyrena.Voice.Contracts;

public interface IVoiceTranscriber : IDisposable
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
    Task DeinitializeAsync(CancellationToken cancellationToken = default);
    bool IsInitialized { get; }
    Task<string?> TranscribeAsync(VoiceArtifact artifact, CancellationToken cancellationToken = default);
}
```

**Behavior**:
- Must call `InitializeAsync` before `TranscribeAsync`.
- Returns the transcribed text, or `null` if transcription fails or produces no result.
- Accepts any `VoiceArtifact` subclass. Implementations may throw if given an unsupported artifact type.
- All operations accept an optional `CancellationToken`.

---

### Models

#### AudioArtifact

Base class for all audio output artifacts (produced by `ITextAudioConverter`, consumed by `IAudioPlayer`).

```csharp
namespace Cyrena.Voice.Models;

public abstract class AudioArtifact
{
}
```

**Notes**:
- Currently an empty abstract base. Implementations should subclass this to add format-specific data (e.g., raw PCM bytes, sample rate, format headers).
- This is the output type of the TTS pipeline stage.

---

#### VoiceArtifact

Base class for all voice input artifacts (produced by `IVoiceRecorder`, consumed by `IVoiceTranscriber`).

```csharp
namespace Cyrena.Voice.Models;

public abstract class VoiceArtifact
{
}
```

**Notes**:
- Currently an empty abstract base. Implementations should subclass this to add format-specific data.
- This is the output type of the recording pipeline stage and the input type of the transcription stage.

---

#### EmptyVoiceArtifact

A sentinel `VoiceArtifact` representing an empty or null recording result.

```csharp
namespace Cyrena.Voice.Models;

public sealed class EmptyVoiceArtifact : VoiceArtifact
{
}
```

**Usage**:
- Returned by `IVoiceRecorder.StopRecording()` when no audio was captured or the recording was cancelled.
- `IVoiceTranscriber` implementations should handle this gracefully (return `null` or empty string).

---

#### StandardWavVoiceArtifact

A `VoiceArtifact` containing standard WAV-format audio data (16-bit mono PCM).

```csharp
namespace Cyrena.Voice.Models;

public sealed class StandardWavVoiceArtifact : VoiceArtifact
{
    public StandardWavVoiceArtifact() { }
    public StandardWavVoiceArtifact(byte[] data)
    {
        Data = data;
    }

    public byte[] Data { get; init; } = [];
}
```

**Properties**:
| Property | Type | Description |
|----------|------|-------------|
| `Data` | `byte[]` | Raw WAV file bytes. Defaults to empty array. |

**Notes**:
- The constructor accepting `byte[]` is the primary way to create instances from recorded audio.
- The `init` accessor means `Data` can be set at initialization time but not mutated afterward.
- This is the standard artifact type expected by most `IVoiceTranscriber` implementations.

---

### Lifecycle Pattern

All voice contracts (`IAudioPlayer`, `ITextAudioConverter`, `IVoiceRecorder`, `IVoiceTranscriber`) follow the same lifecycle pattern:

```
[Created] → InitializeAsync() → [Operational] → DeinitializeAsync() → [Disposed]
                              ↓
                        Dispose()
```

**Rules**:
1. `InitializeAsync` must be called before any operational method (`PlayAsync`, `ConvertAsync`, `StartRecording`, `StopRecording`, `TranscribeAsync`).
2. `IsInitialized` must be `true` after successful initialization and `false` after deinitialization.
3. `DeinitializeAsync` releases resources but does not necessarily dispose the instance.
4. `Dispose` should clean up all resources. It is safe to call `Dispose` without calling `DeinitializeAsync` first.
5. All methods accept `CancellationToken cancellationToken = default`.

---

### Pipeline Flow

The typical voice interaction flow through the chain:

```
┌─────────────────┐     ┌──────────────────┐     ┌─────────────────────┐     ┌─────────────────┐
│  IVoiceRecorder │ ──► │ IVoiceTranscriber│ ──► │ ITextAudioConverter │ ──► │   IAudioPlayer  │
│                 │     │                  │     │                     │     │                 │
│ StartRecording  │     │ TranscribeAsync  │     │    ConvertAsync     │     │    PlayAsync    │
│ StopRecording   │     │   (returns text) │     │  (returns Audio)    │     │                 │
│ (VoiceArtifact) │     │                  │     │                     │     │                 │
└─────────────────┘     └──────────────────┘     └─────────────────────┘     └─────────────────┘
```

`IVoiceChain` holds references to all four stages and can be used to initialize/deinitialize the entire pipeline at once.

---

### Usage for Extension Developers

To create a voice provider extension:

1. Reference `Cyrena.Voice.Core`
2. Implement one or more of the voice contracts
3. Register implementations via the extension's `BuildExtension` method
4. Use the provided artifact types or create custom subclasses

Example artifact subclass:
```csharp
public sealed class Mp3AudioArtifact : AudioArtifact
{
    public byte[] Data { get; init; } = [];
    public int SampleRate { get; init; }
}
```

---

### Dependencies

- No external NuGet package dependencies
- Targets .NET 10.0

---

### Version History

| Version | Notes |
|---------|-------|
| 0.6.0 | Initial release with 5 contracts and 4 artifact models |