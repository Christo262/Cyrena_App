using Markdig;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Microsoft.SemanticKernel.ChatCompletion;
using Cyrena.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Cyrena.Models;
using BootstrapBlazor.Components;
using Cyrena.Attributes;

namespace Cyrena.Components.Shared
{
    public partial class Chat : IDisposable
    {
        [Inject] private IJSRuntime _js { get; set; } = default!;
        [Inject] private ToastService _toasts { get; set; } = default!;
        [Parameter]
        public bool AutoRefocus { get; set; } = true;
        private ElementReference _scrollHost;
        private Markdig.MarkdownPipeline _mdp = default!;

        [KernelInject] private IIterationService _its { get; set; } = default!;
        [KernelInject] private IChatMessageService _msg { get; set; } = default!;
        [KernelInject] private ConnectionInfo _info { get; set; } = default!;
        [KernelInject] private IFileHandlerFactory _files { get; set; } = default!;

        private DotNetObjectReference<Chat>? _dotNetRef;

        protected override void OnInitialized()
        {
            _its_start = _its.OnIterationStart(OnIterationEvent);
            _its_end = _its.OnIterationEnd(OnIterationEvent);
            _dsp_hst = _msg.OnDisplayHistoryChanged(OnDisplayHistoryChanged);
            _dsp_st = _msg.OnStreamToken(OnStreamToken);

            _mdp = new Markdig.MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .Build();
            _its.Input = new ChatMessageContent(_msg.Options.User, "");
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender) return;
            await ScrollToBottomAsync(true);

            _dotNetRef = DotNetObjectReference.Create(this);
            await _js.InvokeVoidAsync("Cyrena.Runtime.registerChatPasteHandler", _area, _dotNetRef);
            await _js.InvokeVoidAsync("Cyrena.Runtime.registerChatDropHandler", _dropZone, _dotNetRef);
        }

        [JSInvokable]
        public async Task OnFilePasted(
            string base64DataUrl,
            string? fileName,
            string? mimeType,
            long size)
        {
            if (_its.Input == null) return;
            if (!_files.HasFileHandlers)
            {
                await _toasts.Error("File Support Error", "Files are not supported in this chat.");
                return;
            }

            if (string.IsNullOrWhiteSpace(fileName))
                fileName = "pasted-file";

            if (string.IsNullOrWhiteSpace(mimeType))
                mimeType = "application/octet-stream";

            var name = EnsureFileNameHasExtension(fileName, mimeType);
            if (!_files.CanHandleType(mimeType, name))
            {
                await _toasts.Error("Not Supported", $"File type is not supported: {mimeType}");
                return;
            }

            var base64Data = base64DataUrl.Contains(',')
                    ? base64DataUrl.Split(',', 2)[1]
                    : base64DataUrl;
            var bytes = Convert.FromBase64String(base64Data);

            var content = await _files.SaveAsync(bytes, mimeType, name);
            if (content == null)
            {
                await _toasts.Error("Not Supported", $"File type is not supported: {mimeType}");
                return;
            }
            _its.Input.Items.Add(content);
            StateHasChanged();
        }

        private string EnsureFileNameHasExtension(string fileName, string mimeType)
        {
            if (Path.HasExtension(fileName))
                return fileName;

            var extension = _files.GetExtension(mimeType);
            return string.IsNullOrEmpty(extension)
                ? fileName
                : $"{fileName}{extension}";
        }

        private async Task Send()
        {
            if (_its.Input == null || string.IsNullOrEmpty(_its.Input.Content)) return;
            
            _its.Iterate();
            await InvokeAsync(StateHasChanged);
            await Task.Delay(100);
            await _js.InvokeVoidAsync("autoGrow", _area, 5);
        }

        private void Cancel() => _its.Cancel();

        public void OnDisplayHistoryChanged(ChatHistory hst)
        {
            _stream = null;
            this.InvokeAsync(async () =>
            {
                StateHasChanged();
                await ScrollToBottomAsync(hst.LastOrDefault()?.Role == AuthorRole.User);
            });
        }

        public void OnHandleComplete()
        {
            _stream = null;
            _its.Input = null;
            this.InvokeAsync(async () =>
            {
                StateHasChanged();
                await _js.InvokeVoidAsync("autoGrow", _area, 5);
                if(AutoRefocus)
                await _area.FocusAsync();
            });
        }

        private void OnItemsAdded(KernelContent[] items)
        {
            if (_its.Input == null) return;
            foreach(var item in items)
                _its.Input.Items.Add(item);
        }

        private async Task RemoveAdditionalItem(KernelContent item)
        {
            if (_its.Input == null) return;
            try
            {
                await _files.CancelAsync(item);
            }catch (Exception ex)
            {
                await _toasts.Error("Error", ex.Message);
            }
            _its.Input.Items.Remove(item);
        }

        public void OnIterationEvent(bool e)
        {
            this.InvokeAsync(async () =>
            {
                StateHasChanged();
                if(!_its.Inferring)
                {
                    await _js.InvokeVoidAsync("autoGrow", _area, 5);
                    if(AutoRefocus)
                    await _area.FocusAsync();
                }
            });
        }

        private string? _stream;
        public void OnStreamToken(string? token)
        {
            _stream += token;
            this.InvokeAsync(async () =>
            {
                this.StateHasChanged();
                await ScrollToBottomAsync(false);
            });
        }

        private async Task ScrollToBottomAsync(bool force, int threshold = 150)
        {
            await _js.InvokeVoidAsync("scrollToBottom", _scrollHost, force, threshold);
        }

        private async Task ComposerKeyDown(KeyboardEventArgs e)
        {
            if (e.Key == "Enter" && !e.ShiftKey)
            {
                await Send();
                return;
            }
            await _js.InvokeVoidAsync("autoGrow", _area, 5);
            StateHasChanged();
        }

        private ElementReference _area = default!;
        private ElementReference _dropZone = default!;

        private IDisposable _its_start = default!;
        private IDisposable _its_end = default!;
        private IDisposable _dsp_hst = default!;
        private IDisposable _dsp_st = default!;
        public void Dispose()
        {
            if (_dotNetRef is not null)
            {
                _ = _js.InvokeVoidAsync("Cyrena.Runtime.unregisterChatPasteHandler", _area);
                _ = _js.InvokeVoidAsync("Cyrena.Runtime.unregisterChatDropHandler", _dropZone);
                _dotNetRef.Dispose();
            }
            _its_end.Dispose();
            _its_start.Dispose();
            _dsp_hst.Dispose();
            _dsp_st.Dispose();
        }
    }
}
