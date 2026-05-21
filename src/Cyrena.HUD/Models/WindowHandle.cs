using Cyrena.Models;
using System.Windows;

namespace Cyrena.HUD.Models
{
    internal class WindowHandle : IWindowHandle
    {
        private Window? _window;
        public WindowHandle(Window window)
        {
            _window = window;
            _window.Closing += _window_Closing;
        }

        private void _window_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_window == null) return;
            _window.Closing -= _window_Closing;
            _window = null;
            Closing?.Invoke(this, EventArgs.Empty);
        }

        public bool Disposed => _window == null;

        public event EventHandler<EventArgs>? Closing;

        public void Close()
        {
            _window?.Close();
        }

        public void Dispose()
        {
            _window?.Close();
        }
    }
}
