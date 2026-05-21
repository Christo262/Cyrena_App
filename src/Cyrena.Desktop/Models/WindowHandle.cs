using Cyrena.Models;
using Photino.NET;

namespace Cyrena.Desktop.Models
{
    internal class WindowHandle : IWindowHandle
    {
        private PhotinoWindow? _window;
        public WindowHandle(PhotinoWindow window)
        {
            _window = window;
            _window.WindowClosing += _window_WindowClosing;
        }

        public bool Disposed => _window == null;

        private bool _window_WindowClosing(object sender, EventArgs e)
        {
            if (_window != null)
                _window.WindowClosing -= _window_WindowClosing;
            _window = null;
            Closing?.Invoke(this, EventArgs.Empty);
            return false;
        }

        public event EventHandler<EventArgs>? Closing;

        public void Close()
        {
            if(_window != null)
                _window.Close();
        }

        public void Dispose()
        {
            if (_window == null) return;
            _window.Close();
        }
    }
}
