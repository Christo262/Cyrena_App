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
            _window.SetMaximized(true);
        }

        public void Minimize()
        {
            if (_window == null) return;
            _window.SetMinimized(true);
        }

        public void Restore()
        {
            if (_window == null) return;
            _window.Minimized = false;
            if (_options.Maximized)
                _window.SetMaximized(true);
            _window.SetHeight(_options.Height);
            _window.SetWidth(_options.Width);
            _restored = true;
        }

        public void SetTransparent(bool b)
        {
            if (_window == null) return;
            _window.SetTransparent(b);
        }

        public void SetTitle(string title)
        {
            if(_window == null) return;
            _window.SetTitle($"Cyréna: {title}");
        }

        public void SetFullScreen(bool b)
        {
            if(_window ==null) return;
            _window.SetFullScreen(b);
        }

        public void SetHeight(int h)
        {
            if (_window == null) return;
            _window.SetHeight(h);
        }

        public void SetWidth(int w)
        {
            if (_window == null) return;
            _window.SetWidth(w);
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

        public async Task<string[]> ShowFileSelect(string title, string name, string[] filters)
        {
            if (_window == null) return [];
            var files = await _window.ShowOpenFileAsync(title, null, false, [(name, filters)]);
            return files;
        }
    }
}
