using Cyrena.Contracts;
using Cyrena.Voice.Contracts;
using Cyrena.Voice.Models;
using Cyrena.Voice.Options;
using System.Text;
using Whisper.net;

namespace Cyrena.Voice.Services
{
    internal class WhisperNetVoiceTranscriber : IVoiceTranscriber
    {
        public const string Key = "whisper.transcriber";
        private readonly ISettingsService _settings;
        public WhisperNetVoiceTranscriber(ISettingsService settings)
        {
            _settings = settings;
        }

        public bool IsInitialized => _processor != null;

        private WhisperFactory? _factory { get; set; }
        private WhisperProcessor? _processor { get; set; }

        public Task DeinitializeAsync(CancellationToken cancellationToken = default)
        {
            _processor?.Dispose();
            _factory?.Dispose();
            _processor = null;
            _factory = null;
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _processor?.Dispose();
            _factory?.Dispose();
            _processor = null;
            _factory = null;
        }

        public Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            if (IsInitialized)
                return Task.CompletedTask;
            var options = _settings.Read<VoiceOptions>(VoiceOptions.Key) ?? new VoiceOptions();
            if (string.IsNullOrEmpty(options.WhisperModelPath))
                throw new InvalidOperationException($"Please configure a Whisper model first in Settings.");
            if (!File.Exists(options.WhisperModelPath))
                throw new FileNotFoundException($"Unable to find Whisper model at {options.WhisperModelPath}");
            _factory = WhisperFactory.FromPath(options.WhisperModelPath);
            _processor = _factory.CreateBuilder()
                .WithLanguageDetection()
                .Build();
            return Task.CompletedTask;
        }

        public async Task<string?> TranscribeAsync(VoiceArtifact artifact, CancellationToken cancellationToken = default)
        {
            if (_processor == null)
                return null;
            if(artifact is EmptyVoiceArtifact)
                return null;
            if(artifact is StandardWavVoiceArtifact wav)
            {
                if (wav.Data.Length == 0)
                    return null;
                using var stream = new MemoryStream(wav.Data);
                var sb = new StringBuilder();
                await foreach (var result in _processor.ProcessAsync(stream, cancellationToken))
                {
                    sb.Append(result.Text);
                }
                return sb.ToString();
            }
            throw new InvalidOperationException($"Whisper expects a artifact of type {typeof(StandardWavVoiceArtifact)} but artifact is {artifact.GetType()}");
        }
    }
}
