using BootstrapBlazor.Components;
using Cyrena.Contracts;
using Microsoft.AspNetCore.Components;

namespace Cyrena.Services
{
    internal class DisplayService : IDisplayService
    {
        private readonly SemaphoreSlim _dialogLock = new(1, 1);

        private DialogService? _dialog;
        private ToastService? _toasts;
        private NavigationManager? _nav;
        public async Task<DialogResult> ShowModal<TComponent>(ResultDialogOption option, Dialog? dialog = null) where TComponent : IComponent, IResultDialog
        {
            if (_dialog == null)
                throw new Exception("DialogService not set");
            await _dialogLock.WaitAsync();
            try
            {
                return await _dialog.ShowModal<TComponent>(option, dialog);
            }
            finally
            {
                await Task.Delay(1000);
                _dialogLock.Release();
            }
        }
        public async Task<DialogResult> ShowModal(string title, string content, ResultDialogOption? option = null, Dialog? dialog = null)
        {
            if (_dialog == null)
                throw new Exception("DialogService not set");
            await _dialogLock.WaitAsync();
            try
            {
                return await _dialog.ShowModal(title, content, option, dialog);
            }
            finally
            {
                await Task.Delay(1000);
                _dialogLock.Release();
            }
        }

        public Task ShowToast(ToastOption option, ToastContainer? toastContainer = null)
        {
            if(_toasts == null)
                throw new Exception("ToastService not set");
            return _toasts.Show(option, toastContainer);
        }

        public Task ShowErrorToast(string? title = null, string? content = null, bool autoHide = true)
        {
            if (_toasts == null)
                throw new Exception("ToastService not set");
            return _toasts.Error(title, content, autoHide);
        }

        public Task ShowWarnToast(string? title = null, string? content = null, bool autoHide = true)
        {
            if (_toasts == null)
                throw new Exception("ToastService not set");
            return _toasts.Warning(title, content, autoHide);
        }

        public Task ShowSuccessToast(string? title = null, string? content = null, bool autoHide = true)
        {
            if (_toasts == null)
                throw new Exception("ToastService not set");
            return _toasts.Success(title, content, autoHide);
        }

        public Task ShowInfoToast(string? title = null, string? content = null, bool autoHide = true)
        {
            if (_toasts == null)
                throw new Exception("ToastService not set");
            return _toasts.Information(title, content, autoHide);
        }

        public void NavigateTo(string url)
        {
            if(_nav ==  null)
                throw new Exception("NavigationManager not set");
            _nav.NavigateTo(url);
        }

        internal void SetServices(DialogService dialog, ToastService toasts, NavigationManager nav)
        {
            _dialog = dialog;
            _toasts = toasts;
            _nav = nav;
        }
    }
}
