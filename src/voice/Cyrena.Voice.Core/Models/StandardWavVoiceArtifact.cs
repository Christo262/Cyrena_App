namespace Cyrena.Voice.Models
{
    /// <summary>
    /// Standard .wav, 16-bit mono
    /// </summary>
    public sealed class StandardWavVoiceArtifact : VoiceArtifact
    {
        public StandardWavVoiceArtifact() { }
        public StandardWavVoiceArtifact(byte[] data)
        {
            Data = data;
        }

        public byte[] Data { get; init; } = [];
    }
}
