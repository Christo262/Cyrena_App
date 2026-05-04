using BootstrapBlazor.Components;
using Cyrena.Contracts;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace Cyrena.Components.Tools
{
    public partial class ClearChat
    {
        private IChatMessageService _chat = default!;
        private IIterationService _its = default!;
        [Inject] private DialogService _dialog { get; set; } = default!;
        protected override void OnInitialized()
        {
            _chat = Kernel.Services.GetRequiredService<IChatMessageService>();
            _its = Kernel.Services.GetRequiredService<IIterationService>();
        }

        private async Task ClearChatAsync()
        {
            if (_its.Inferring) return;
            var r = await _dialog.ShowModal("Clear Chat", "Are you sure you want to delete the chat history?", new ResultDialogOption()
            {
                Size = Size.Medium
            });
            if(r == DialogResult.Yes)
            await _chat.ClearHistoryAsync();
        }
    }
}
