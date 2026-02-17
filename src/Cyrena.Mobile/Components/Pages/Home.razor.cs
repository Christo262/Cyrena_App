using BootstrapBlazor.Components;
using Cyrena.Contracts;
using Cyrena.Mobile.Models;
using Cyrena.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace Cyrena.Mobile.Components.Pages
{
    public partial class Home
    {
        [Inject] private ISettingsService _settings { get; set; } = default!;
        [Inject] private IKernelController _kernels { get; set; } = default!;
        [Inject] private NavigationManager _nav { get; set; } = default!;
        [Inject] private ToastService _toasts { get; set; } = default!;
        [Inject] private IJSRuntime _js { get; set; } = default!;

        private ChatConfiguration? _model;
        private string? _input { get; set; }

        protected override void OnInitialized()
        {
            var options = _settings.Read<MobileOptions>(MobileOptions.Key);
            if (options == null || string.IsNullOrEmpty(options.DefaultConnectionId))
                return;
            _model = new ChatConfiguration()
            {
                Id = Guid.NewGuid().ToString(),
                AssistantModeId = IAssistantMode.AssistantModeDefault,
                ConnectionId = options.DefaultConnectionId,
            };
            _model[ChatConfiguration.Icon] = "bi bi-chat-left-quote";
        }

        private async Task Send()
        {
            if (string.IsNullOrEmpty(_input) || _model == null)
                return;
            try
            {
                var kernel = await _kernels.Create(_model);
                var chat = kernel.Services.GetRequiredService<IChatMessageService>();
                kernel.Services.GetRequiredService<IIterationService>().Iterate(chat.Options.User, _input, kernel);
                _nav.NavigateTo($"converse/{_model.Id}");
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
    }
}
