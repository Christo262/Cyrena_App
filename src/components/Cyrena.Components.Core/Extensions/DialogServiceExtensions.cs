using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Cyrena.Extensions
{
    public static class DialogServiceExtensions
    {
        public static async Task<bool> ShowDialogAsync<TComponent>(this IDialogService dialog, string title, DialogParameters parameters, MaxWidth maxWidth = MaxWidth.Medium)
            where TComponent : ComponentBase
        {
            var reference = await dialog.ShowAsync<TComponent>(title, parameters, new DialogOptions()
            {
                FullWidth = true,
                MaxWidth = maxWidth
            });
            var result = await reference.Result;
            if(result == null || result.Canceled == true)
                return false;
            return true;
        }
    }
}
