using Cyrena.Attributes;
using Cyrena.Contracts;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Cyrena.Components.Tools
{
    public partial class ClearChat
    {
        [KernelInject] private IChatMessageService _chat { get; set; } = default!;
        [KernelInject] private IIterationService _its { get; set; } = default!;
        [Inject] private IDialogService _dialog { get; set; } = default!;
        private async Task ClearChatAsync()
        {
            if (_its.Inferring) return;
            var r = await _dialog.ShowMessageBoxAsync("Clear Chat", "Are you sure you want to delete the chat history?", "Yes", "No", options:new DialogOptions()
            {
                MaxWidth = MaxWidth.Small,
                FullWidth = true
            });
            if(r == true)
            await _chat.ClearHistoryAsync();
        }
    }
}
