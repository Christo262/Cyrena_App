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

        private IIterationService _its = default!;
        private IChatMessageService _msg = default!;
        private ConnectionInfo _info = default!;

        private DotNetObjectReference<Chat>? _dotNetRef;

        protected override void OnInitialized()
        {
            _info = Kernel.Services.GetRequiredService<ConnectionInfo>();
            _its = Kernel.Services.GetRequiredService<IIterationService>();
            _msg = Kernel.Services.GetRequiredService<IChatMessageService>();
            _its_start = _its.OnIterationStart(OnIterationEvent);
            _its_end = _its.OnIterationEnd(OnIterationEvent);
            _dsp_hst = _msg.OnDisplayHistoryChanged(OnDisplayHistoryChanged);
            _dsp_st = _msg.OnStreamToken(OnStreamToken);

            _mdp = new Markdig.MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .Build();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender) return;
            await ScrollToBottomAsync(true);

            _dotNetRef = DotNetObjectReference.Create(this);
            await _js.InvokeVoidAsync("Cyrena.Runtime.registerChatPasteHandler", _area, _dotNetRef);
        }

        [JSInvokable]
        public void OnImagePasted(string base64DataUrl, string mimeType)
        {
            var info = Kernel.GetRequiredService<ConnectionInfo>();
            if (!info.SupportImages)
            {
                _toasts.Error("Not Supported", "This model does not support images.");
                return;
            }
            if (string.IsNullOrEmpty(mimeType))
                mimeType = "image/png";
            var extension = mimeType switch
            {
                "image/png" => "png",
                "image/jpeg" => "jpg",
                "image/gif" => "gif",
                "image/webp" => "webp",
                _ => "png"
            };

            var base64Data = base64DataUrl.Contains(',')
                ? base64DataUrl.Split(',')[1]
                : base64DataUrl;

            var bytes = Convert.FromBase64String(base64Data);
            var imageContent = new ImageContent(bytes, mimeType);
            var additionalContent = new AdditionalMessageContent($"pasted-image.{extension}", imageContent);

            _items.Add(additionalContent);
            InvokeAsync(StateHasChanged);
        }

        private List<AdditionalMessageContent> _items = new List<AdditionalMessageContent>();
        private async Task Send()
        {
            if (string.IsNullOrWhiteSpace(_its.Input)) return;

            _its.Iterate(AuthorRole.User, Kernel, _items.ToArray());
            _items.Clear();
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
            _items.Clear();
            this.InvokeAsync(async () =>
            {
                StateHasChanged();
                await _js.InvokeVoidAsync("autoGrow", _area, 5);
                if(AutoRefocus)
                await _area.FocusAsync();
            });
        }

        private void OnItemsAdded(AdditionalMessageContent[] items)
        {
            _items.AddRange(items);
        }

        private void RemoveAdditionalItem(AdditionalMessageContent item)
        {
            _items.Remove(item);
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
        private async Task AutoGrow(ChangeEventArgs e)
        {
            _its.Input = e.Value?.ToString() ?? "";
            await _js.InvokeVoidAsync("autoGrow", _area, 5);
            StateHasChanged();
        }

        private IDisposable _its_start = default!;
        private IDisposable _its_end = default!;
        private IDisposable _dsp_hst = default!;
        private IDisposable _dsp_st = default!;
        public void Dispose()
        {
            if (_dotNetRef is not null)
            {
                _ = _js.InvokeVoidAsync("Cyrena.Runtime.unregisterChatPasteHandler", _area);
                _dotNetRef.Dispose();
            }
            _its_end.Dispose();
            _its_start.Dispose();
            _dsp_hst.Dispose();
            _dsp_st.Dispose();
        }
    }
}
