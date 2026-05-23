using Avalonia.Controls;
using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Options;

namespace Cyrena.Shell
{
    public partial class MainWindow : Window
    {
        public string Url { get; set; }
        private readonly ISettingsService _settings;
        public MainWindow()
        {
            _settings = CyrenaRuntime.CreateSettings();
            var options = _settings.Read<ApplicationOptions>(ApplicationOptions.Key) ?? new ApplicationOptions();
            Url = $"http://localhost:{options.ServerPort}";
            InitializeComponent();
            DataContext = this;

            this.Width = options.Width;
            this.Height = options.Height;
            this.Resized += MainWindow_Resized;

#if DEBUG
            
#endif
        }

        private void MainWindow_Resized(object? sender, WindowResizedEventArgs e)
        {
            var options = _settings.Read<ApplicationOptions>(ApplicationOptions.Key) ?? new ApplicationOptions();
            options.Width = this.Width;
            options.Height = this.Height;
            _settings.Save(ApplicationOptions.Key, options);
        }

        protected override void OnClosing(WindowClosingEventArgs e)
        {
            e.Cancel = true;
            Hide();
            base.OnClosing(e);
        }

        private void WebView_NavigationCompleted(object? sender, WebViewNavigationCompletedEventArgs args)
        {
            if (args.IsSuccess)
            {
                // Navigation completed successfully
            }
        }
    }
}