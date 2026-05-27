using Cyrena.Options;
using Cyrena.Shell.Contracts;

namespace Cyrena.Shell.Services
{
    public class WindowsWindowService : IWindowService
    {
        private MainWindow? _main { get; set; }

        public void Show(ApplicationOptions options)
        {
            if(_main == null)
            {
                _main = new MainWindow();
                _main.Closing += _main_Closing;
            }
            _main.Width = options.Width;
            _main.Height = options.Height;
            _main.ShowInTaskbar = true;
            _main.WindowStartupLocation = Avalonia.Controls.WindowStartupLocation.CenterScreen;
            _main.Show();
        }

        private void _main_Closing(object? sender, Avalonia.Controls.WindowClosingEventArgs e)
        {
            if (_main == null) return;
            e.Cancel = true;
            _main.ShowInTaskbar = false;
            _main.Hide();
        }

        public void Dispose()
        {
            if(_main != null )
            _main.Closing -= _main_Closing;
        }
    }
}
