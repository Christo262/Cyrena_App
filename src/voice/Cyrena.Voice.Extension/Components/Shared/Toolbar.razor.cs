using BootstrapBlazor.Components;
using Cyrena.Attributes;
using Cyrena.Contracts;
using Cyrena.Voice.Contracts;
using Cyrena.Voice.Options;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Text;

namespace Cyrena.Voice.Components.Shared
{
    public partial class Toolbar : IDisposable
    {
        [Inject] private IServiceProvider _services { get; set; } = default!;
        [Inject] private ISettingsService _settings { get; set; } = default!;
        [KernelInject] private IIterationService _its { get; set; } = default!;
        [KernelInject] private IChatMessageService _chat { get; set; } = default!;
        [Inject] private ToastService _toasts { get; set; } = default!;

        private StreamingSpeechFormatter _speechFormatter { get; set; } = default!;

        private IVoiceChain? _chain { get; set; }
        private CancellationTokenSource _cts = default!;

        private bool _initialized => _chain is not null;
        private bool _recording => _chain is not null && _chain.Recorder.IsRecording;
        private bool _processing { get; set; }
        private bool _voice_mode { get; set; }

        private IDisposable? _its_start { get; set; }
        private IDisposable? _its_end { get; set; }
        private IDisposable? _stream { get; set; }
        private IDisposable? _hst { get; set; }

        private async Task Exit()
        {
            _voice_mode = false;
            await _cts.CancelAsync();
            if(_chain != null)
                await _chain.DeinitializeAsync();
            _its_start?.Dispose();
            _its_end?.Dispose();
            _stream?.Dispose();
            _hst?.Dispose();

            _its_start = null;
            _its_end = null;
            _stream = null;
            _hst = null;

            _processing = false;
            _playback.Clear();
        }

        protected override void OnInitialized()
        {
            _speechFormatter = new StreamingSpeechFormatter();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender) return;
            try
            {
                var chains = _services.GetServices<IVoiceChain>();
                var options = _settings.Read<WebViewVoiceOptions>(WebViewVoiceOptions.Key) ?? new WebViewVoiceOptions();
                _chain = chains.FirstOrDefault(x => x.Id == options.DefaultVoiceChain);
                if (_chain == null)
                    throw new InvalidOperationException($"Unable to configure voice mode");
                this.StateHasChanged();
            }
            catch (Exception ex)
            {
                await _toasts.Error("Voice Error", ex.Message);
            }
        }

        private async Task StartRecording()
        {
            try
            {
                if (!_initialized) return;
                if (!_voice_mode)
                {
                    if (!_chain!.IsInitialized)
                        await _chain.InitializeAsync();
                    if (_cts != null &&!_cts.IsCancellationRequested)
                    {
                        await _cts.CancelAsync();
                        _cts.Dispose();
                    }
                    _cts = new CancellationTokenSource();
                    _its_start = _its.OnIterationStart(e =>
                    {
                        this.InvokeAsync(StateHasChanged);
                    });
                    _its_end = _its.OnIterationEnd(e =>
                    {
                        this.InvokeAsync(StateHasChanged);
                    });
                    _ = Task.Run(RunQueue, _cts.Token);
                    _voice_mode = true;
                }
                if (_chain!.Recorder.IsRecording)
                    await _toasts.Warning("Voice", "Already recording");
                await _chain.Recorder.StartRecording(_cts.Token);
                this.StateHasChanged();
            }
            catch (Exception ex)
            {
                await _toasts.Error("Voice Error", ex.Message);
            }
        }

        private ConcurrentQueue<string> _playback = new ConcurrentQueue<string>();
        private async Task StopRecording()
        {
            try
            {
                if (!_recording || !_initialized) return;
                _processing = true;
                var voice = await _chain!.Recorder.StopRecording(_cts.Token);
                this.StateHasChanged();
                var text = await _chain.Transcriber.TranscribeAsync(voice, _cts.Token);
                if (!string.IsNullOrEmpty(text))
                {
                    if (_stream == null)
                    {
                        _stream = _chat.OnStreamToken(e =>
                        {
                            foreach (var sentence in _speechFormatter.Push(e))
                                _playback.Enqueue(sentence);
                        });
                        _hst = _chat.OnDisplayHistoryChanged(cht =>
                        {
                            var lst = cht.LastOrDefault();
                            if (lst != null && lst.Role == _chat.Options.Assistant)
                                foreach (var sentence in _speechFormatter.Flush())
                                    _playback.Enqueue(sentence);
                        });
                    }
                    if (_its.Input == null)
                        _its.Input = new Microsoft.SemanticKernel.ChatMessageContent(_chat.Options.User, text);
                    else
                        _its.Input.Content = text;
                    _its.Iterate();
                }
                this.StateHasChanged();
            }
            catch (Exception ex)
            {
                await _toasts.Error("Voice Error", ex.Message);
            }
        }

        private async Task RunQueue()
        {
            while (!_cts.IsCancellationRequested)
            {
                if(!_playback.TryDequeue(out var txt))
                {
                    await Task.Yield();
                    continue;
                }
                try
                {
                    var audio = await _chain!.Converter.ConvertAsync(txt!, _cts.Token);
                    await _chain.Player.PlayAsync(audio, _cts.Token);
                }catch (Exception ex)
                {
                    await this.InvokeAsync(async () =>
                    {
                        await _toasts.Error("Voice Error", ex.Message);
                    });
                }
                finally
                {
                    if (_playback.Count == 0)
                    {
                        _processing = false;
                        await this.InvokeAsync(StateHasChanged);
                    }
                }
                await Task.Yield();
            }
        }

        public void Dispose()
        {
            if(_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
            }
            _its_start?.Dispose();
            _its_end?.Dispose();
            _stream?.Dispose();
            _its_start = null;
            _its_end = null;
            _stream = null;
            _hst = null;
        }

        private readonly StringBuilder _ttsBuffer = new();
        private CancellationTokenSource? _ttsDebounce;

        private void HandleStreamToken(string? token)
        {
            if (string.IsNullOrEmpty(token))
                return;

            _ttsBuffer.Append(token);

            _ttsDebounce?.Cancel();
            _ttsDebounce = new CancellationTokenSource();

            var ct = _ttsDebounce.Token;

            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(700, ct);

                    var text = _ttsBuffer.ToString().Trim();

                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        _ttsBuffer.Clear();
                        _playback.Enqueue(text);
                    }
                }
                catch (TaskCanceledException) { }
            });
        }
    }
}
