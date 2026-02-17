using Markdig;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Microsoft.SemanticKernel.ChatCompletion;
using Cyrena.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Cyrena.Models;

namespace Cyrena.Components.Shared
{
    public partial class Chat : IDisposable
    {
        [Inject] private IJSRuntime _js { get; set; } = default!;

        private ElementReference _scrollHost;
        private string? _input { get; set; }
        private Markdig.MarkdownPipeline _mdp = default!;

        private IIterationService _its = default!;
        private IChatMessageService _msg = default!;
        private IEnumerable<ICapability> _caps = default!;

        protected override void OnInitialized()
        {
            _caps = Kernel.Services.GetServices<ICapability>();
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
        }

        private List<AdditionalMessageContent> _items = new List<AdditionalMessageContent>();
        private async Task Send()
        {
            if (_its.Inferring) return;
            if (string.IsNullOrWhiteSpace(_input)) return;

            var userText = _input.Trim();
            _its.Iterate(AuthorRole.User, userText, Kernel, _items.ToArray());
            _input = string.Empty; // Use empty string instead of null
            _items.Clear();
            await InvokeAsync(StateHasChanged);
            await Task.Delay(100);
            await _js.InvokeVoidAsync("autoGrow", _area, 5);
        }

        public void OnDisplayHistoryChanged(ChatHistory hst)
        {
            _stream = null;
            this.InvokeAsync(async () =>
            {
                StateHasChanged();
                await ScrollToBottomAsync(hst.Last().Role == AuthorRole.User);
            });
        }

        public void OnHandleComplete()
        {
            _stream = null;
            _input = null;
            _items.Clear();
            this.InvokeAsync(async () =>
            {
                StateHasChanged();
                await _js.InvokeVoidAsync("autoGrow", _area, 5);
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
            this.InvokeAsync(StateHasChanged);
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
            _input = e.Value?.ToString() ?? "";
            await _js.InvokeVoidAsync("autoGrow", _area, 5);
            StateHasChanged();
        }

        private IDisposable _its_start = default!;
        private IDisposable _its_end = default!;
        private IDisposable _dsp_hst = default!;
        private IDisposable _dsp_st = default!;
        public void Dispose()
        {
            _its_end.Dispose();
            _its_start.Dispose();
            _dsp_hst.Dispose();
            _dsp_st.Dispose();
        }
    }
}
