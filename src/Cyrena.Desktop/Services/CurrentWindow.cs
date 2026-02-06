using Cyrena.Contracts;
using Photino.NET;

namespace Cyrena.Desktop.Services
{
    internal class WindowOptions
    {
        internal const string Key = "photino.window";

        public bool Maximized { get; set; }
        public int Width { get; set; } = 1200;
        public int Height { get; set; } = 800;
    }

    internal class CurrentWindow : ICurrentWindow
    {
        private PhotinoWindow? _window;
        private bool _restored;
        private readonly ISettingsService _settings;
        private readonly WindowOptions _options;
        public CurrentWindow(ISettingsService settings)
        {
            _settings = settings;
            _options = _settings.Read<WindowOptions>(WindowOptions.Key) ?? new WindowOptions();
        }

        internal void SetWindow(PhotinoWindow window)
        {
            _window = window;
            _window.WindowMaximized += _window_WindowMaximized;
            _window.WindowSizeChanged += _window_WindowSizeChanged;
        }

        private void _window_WindowSizeChanged(object? sender, System.Drawing.Size e)
        {
            if (!_restored) return;
            _options.Height = e.Height;
            _options.Width = e.Width;
            Save();
        }

        private void _window_WindowMaximized(object? sender, EventArgs e)
        {
            if (!_restored) return;
            _options.Maximized = true;
            Save();
        }

        public void Close()
        {
            if(_window == null) return;
            _window.Close();
        }

        public void Maximize()
        {
            if (_window == null) return;
            _window.Maximized = true;
        }

        public void Minimize()
        {
            if (_window == null) return;
            _window.Minimized = true;
        }

        public void Restore()
        {
            if (_window == null) return;
            _window.Minimized = false;
            _window.Maximized = _options.Maximized;
            _window.Height = _options.Height;
            _window.Width = _options.Width;
            _window.Center();
            _restored = true;
        }

        public void Dispose()
        {
            if(_window == null) return;
            _window.WindowMaximized -= _window_WindowMaximized;
            _window.WindowSizeChanged -= _window_WindowSizeChanged;
        }

        private void Save()
        {
            _settings.Save(WindowOptions.Key, _options);
        }
    }
}
