using Cyrena.Extensa.Models;
using Cyrena.Extensions;
using Cyrena.Options;
using Cyrena.Voice.Components.Shared;
using Cyrena.Voice.Contracts;
using Cyrena.Voice.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Cyrena.Voice
{
    public class VoiceExtension : Extension
    {
        public override void BuildExtension(CyrenaBuilder builder)
        {
            builder.Services.AddKeyedScoped<IVoiceRecorder, WebViewVoiceRecorder>(WebViewVoiceRecorder.Key);
            builder.Services.AddKeyedScoped<IVoiceTranscriber, WhisperNetVoiceTranscriber>(WhisperNetVoiceTranscriber.Key);
            builder.Services.AddKeyedScoped<ITextAudioConverter, WebViewTextAudioConverter>(WebViewTextAudioConverter.Key);
            builder.Services.AddKeyedScoped<IAudioPlayer, WebViewAudioPlayer>(WebViewAudioPlayer.Key);
            builder.Services.AddScoped<IVoiceChain, WebViewWhisperVoiceChain>();
            
            builder.Services.AddKeyedScoped<ITextAudioConverter, ESpeakTextAudioConverter>(ESpeakTextAudioConverter.Key);
            builder.Services.AddScoped<IVoiceChain, ESpeakWhisperVoiceChain>();

            builder.AddSettingsComponent<Settings>("Voice", 10);
            builder.AddAssistantPlugin<VoiceModeAssistantPlugin>();
        }
    }
}
