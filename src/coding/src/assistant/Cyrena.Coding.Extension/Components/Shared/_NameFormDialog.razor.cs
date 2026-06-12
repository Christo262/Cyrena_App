using MudBlazor;

namespace Cyrena.Coding.Components.Shared;

public static class NameFormDialogExtensions
{
    public static async Task<string?> ShowNameFormDialog(this IDialogService dialog, string label, string? value)
    {
        var rf = await dialog.ShowAsync<_NameFormDialog>(label, new DialogParameters()
        {
            { nameof(_NameFormDialog.Value), value }
        }, new DialogOptions()
        {
            FullWidth = true,
            MaxWidth = MaxWidth.Small
        });
        var result = await rf.Result;
        if (result is { Canceled: false } && !string.IsNullOrEmpty(result.Data?.ToString()))
            return result.Data.ToString();
        return null;
    }
}