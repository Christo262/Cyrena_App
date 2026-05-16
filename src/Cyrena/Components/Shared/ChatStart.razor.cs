using BootstrapBlazor.Components;
using Cyrena.Contracts;
using Cyrena.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Microsoft.SemanticKernel;

namespace Cyrena.Components.Shared
{
    public partial class ChatStart : IDisposable
    {
        [Inject] private IKernelController _kernels { get; set; } = default!;
        [Inject] private NavigationManager _nav { get; set; } = default!;
        [Inject] private ToastService _toasts { get; set; } = default!;
        [Inject] private IJSRuntime _js { get; set; } = default!;
        [Inject] private ISetupService _setup { get; set; } = default!;

        private ChatConfiguration? _model;
        private Modal? _config;
        private string? _input { get; set; }

        protected override void OnInitialized()
        {
            _setup.OnDefaultConnectionSet += _setup_OnDefaultConnectionSet;
        }

        protected override async Task OnInitializedAsync()
        {
            await RefreshModel();
        }

        private void _setup_OnDefaultConnectionSet(object? sender, EventArgs e)
        {
            this.InvokeAsync(async () => await RefreshModel());
        }

        private async Task RefreshModel()
        {
            _model = null;
            this.StateHasChanged();
            await Task.Delay(50);
            var connectionId = await _setup.GetDefaultConnection();
            if (string.IsNullOrEmpty(connectionId))
                return;
            _model = new ChatConfiguration()
            {
                Id = Guid.NewGuid().ToString(),
                AssistantModeId = IAssistantMode.AssistantModeDefault,
                ConnectionId = connectionId,
            };
            _model[ChatConfiguration.Icon] = "bi bi-chat-left-quote";
            _input = null;
            this.StateHasChanged();
        }

        private async Task Settings()
        {
            if (_config != null)
                await _config.Show();
        }

        private async Task Send()
        {
            if (string.IsNullOrEmpty(_input) || _model == null)
                return;
            try
            {
                var kernel = await _kernels.Create(_model);
                var chat = kernel.Services.GetRequiredService<IChatMessageService>();
                var its = kernel.Services.GetRequiredService<IIterationService>();
                its.Input = _input;
                its.Iterate(chat.Options.User, kernel);
                var url = $"converse/{_model.Id}";
                await RefreshModel();
                _nav.NavigateTo(url);
            }
            catch (Exception ex)
            {
                await _toasts.Error("Error", ex.Message);
            }
        }

        private async Task StartEmpty()
        {
            if (_model == null) return;
            try
            {
                var kernel = await _kernels.Create(_model);
                var url = $"converse/{_model.Id}";
                await RefreshModel();
                _nav.NavigateTo(url);
            }
            catch (Exception ex)
            {
                await _toasts.Error("Error", ex.Message);
            }
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

        public void Dispose()
        {
            _setup.OnDefaultConnectionSet -= _setup_OnDefaultConnectionSet;
        }
    }
}
