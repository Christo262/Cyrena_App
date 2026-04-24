using BootstrapBlazor.Components;
using Microsoft.AspNetCore.Components;

namespace Cyrena.Contracts
{
    //Kernel Locked
    public interface IDisplayService
    {
        Task<DialogResult> ShowModal<TComponent>(ResultDialogOption option, Dialog? dialog = null) where TComponent : IComponent, IResultDialog;
        Task<DialogResult> ShowModal(string title, string content, ResultDialogOption? option = null, Dialog? dialog = null);
        Task ShowToast(ToastOption option, ToastContainer? toastContainer = null);
        Task ShowErrorToast(string? title = null, string? content = null, bool autoHide = true);
        Task ShowWarnToast(string? title = null, string? content = null, bool autoHide = true);
        Task ShowSuccessToast(string? title = null, string? content = null, bool autoHide = true);
        Task ShowInfoToast(string? title = null, string? content = null, bool autoHide = true);
        void NavigateTo(string url);
    }
}
