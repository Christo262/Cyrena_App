using BootstrapBlazor.Components;
using Cyrena.Attributes;
using Cyrena.Contracts;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace Cyrena.Components.Tools
{
    public partial class ClearChat
    {
        [KernelInject] private IChatMessageService _chat { get; set; } = default!;
        [KernelInject] private IIterationService _its { get; set; } = default!;
        [Inject] private DialogService _dialog { get; set; } = default!;
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
