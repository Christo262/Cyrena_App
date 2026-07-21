using Cyrena.Voice.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Cyrena.Voice.Services;

internal class ESpeakWhisperVoiceChain : IVoiceChain
{
    public const string Key = "espeak.whisper";
        public ESpeakWhisperVoiceChain(
            [FromKeyedServices(WebViewVoiceRecorder.Key)]IVoiceRecorder recorder,
            [FromKeyedServices(WhisperNetVoiceTranscriber.Key)] IVoiceTranscriber transcriber,
            [FromKeyedServices(ESpeakTextAudioConverter.Key)] ITextAudioConverter converter,
            [FromKeyedServices(WebViewAudioPlayer.Key)] IAudioPlayer player)
        {
            Recorder = recorder;
            Transcriber = transcriber;
            Converter = converter;
            Player = player;
        }
        public string Id => Key;
        public string Name => "Whisper & ESpeak";
        public string? Description => "Uses ESpeak & Whisper locally for STT/TTS";
        public bool IsInitialized => Recorder.IsInitialized && Transcriber.IsInitialized && Converter.IsInitialized && Player.IsInitialized;

        public IVoiceRecorder Recorder { get; }

        public IVoiceTranscriber Transcriber { get; }

        public ITextAudioConverter Converter { get; }

        public IAudioPlayer Player { get; }

        public async Task DeinitializeAsync(CancellationToken cancellationToken = default)
        {
            if (Recorder.IsInitialized)
                await Recorder.DeinitializeAsync(cancellationToken);
            if (Transcriber.IsInitialized)
                await Transcriber.DeinitializeAsync(cancellationToken);
            if (Converter.IsInitialized)
                await Converter.DeinitializeAsync(cancellationToken);
            if (Player.IsInitialized)
                await Player.DeinitializeAsync(cancellationToken);
        }

        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            if(!Recorder.IsInitialized)
                await Recorder.InitializeAsync(cancellationToken);
            if (!Transcriber.IsInitialized)
                await Transcriber.InitializeAsync(cancellationToken);
            if(!Converter.IsInitialized)
                await Converter.InitializeAsync(cancellationToken);
            if(!Player.IsInitialized)
                await Player.InitializeAsync(cancellationToken);
        }
}