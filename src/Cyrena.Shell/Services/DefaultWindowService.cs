using Avalonia.Controls;
using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Options;

namespace Cyrena.Shell.Services
{
    public class DefaultWindowService : IWindowLauncher
    {
        private MainWindow? _main { get; set; }

        private void _main_SizeChanged(object? sender, Avalonia.Controls.SizeChangedEventArgs e)
        {
            var settings = CyrenaRuntime.CreateSettings();
            var options = settings.Read<ApplicationOptions>(ApplicationOptions.Key) ?? new ApplicationOptions();
            options.Width = (int)e.NewSize.Width;
            options.Height = (int)e.NewSize.Height;
            settings.Save(ApplicationOptions.Key, options);
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

        public void Show(string url, int width, int height, string title = "Cyréna")
        {
            if(!Avalonia.Threading.Dispatcher.UIThread.CheckAccess())
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(() => Show(url, width, height));
                return;
            }
            if(_main == null)
            {
                _main = new MainWindow();
                _main.Closing += _main_Closing;
                _main.Width = width;
                _main.Height = height;
                _main.ShowInTaskbar = true;
                _main.SizeChanged += _main_SizeChanged;
                _main.WindowStartupLocation = Avalonia.Controls.WindowStartupLocation.CenterScreen;
                _main.Show();
                return;
            }

            var unmanaged = new MainWindow(url, width, height);
            unmanaged.Show();
        }
    }
}
