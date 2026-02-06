using Markdig;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Microsoft.SemanticKernel.ChatCompletion;
using Cyrena.Contracts;

namespace Cyrena.Components.Shared
{
    public partial class Chat : IConversationListener
    {
        [Inject] private IJSRuntime _js { get; set; } = default!;

        private ElementReference _scrollHost;

        private string? _input { get; set; }
        private Markdig.MarkdownPipeline _mdp = default!;

        protected override void OnInitialized()
        {
            Context.AttachListener(this);
            _mdp = new Markdig.MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .Build();
        }

        private async Task Send()
        {
            if (Context.Handling) return;
            if (string.IsNullOrWhiteSpace(_input)) return;
            var userText = _input.Trim();
            Context.Handle(AuthorRole.User, userText);
            _input = null;
            await _js.InvokeVoidAsync("autoGrow", _area, 5);
            this.StateHasChanged();
        }

        public void OnDisplayHistoryChanged()
        {
            _stream = null;
            this.InvokeAsync(async () =>
            {
                StateHasChanged();
                await ScrollToBottomAsync(Context.DisplayHistory.Last().Role == AuthorRole.User);
            });
        }

        public void OnHandleComplete()
        {
            _stream = null;
            _input = null;
            this.InvokeAsync(async () =>
            {
                StateHasChanged();
                await _js.InvokeVoidAsync("autoGrow", _area, 5);
                await _area.FocusAsync();
            });
        }

        public void OnHandleStart()
        {
            this.InvokeAsync(StateHasChanged);
        }

        private string? _stream;
        public void OnStreamToken(string token)
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
        }

        private ElementReference _area;
        private async Task AutoGrow(ChangeEventArgs e)
        {
            _input = e.Value?.ToString() ?? "";
            await _js.InvokeVoidAsync("autoGrow", _area, 5);
        }


    }
}
