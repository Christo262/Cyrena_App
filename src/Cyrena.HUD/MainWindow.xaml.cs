using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.HUD.Components.Shared;
using Cyrena.HUD.Options;
using Cyrena.HUD.Services;
using Microsoft.AspNetCore.Components.WebView;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace Cyrena.HUD
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private HotkeyService? _hotkeyService;
        private readonly ISettingsService _settings;
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            mainView.BlazorWebViewInitialized += OnBlazorWebViewInitialized;

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddWpfBlazorWebView();
            serviceCollection.AddBootstrapBlazor();
#if DEBUG
            serviceCollection.AddBlazorWebViewDeveloperTools();
#endif
            serviceCollection.AddSingleton(this);
            var builder = serviceCollection.AddCyrenaRuntime()
                .AddComponents()
                .AddOllama()
                .AddOpenAI()
                .AddTavily()
                .AddApiReferencePages()
                .AddDeveloperRuntime()
                .AddDotnetDevelopment()
                .AddPlatformIO()
                .AddArduinoIDE();
            var files = new FileDialog();
            builder.Services.AddSingleton<IFileDialog>(files);
            builder.AddSettingsComponent<Defaults>();
            builder.Build();
            var sp = serviceCollection.BuildServiceProvider();
            Resources.Add("services", sp);
            _settings = sp.GetRequiredService<ISettingsService>();
        }

        private void OnBlazorWebViewInitialized(object? sender, BlazorWebViewInitializedEventArgs e)
        {
            e.WebView.DefaultBackgroundColor = System.Drawing.Color.Transparent;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            OnHotkeySet();
        }

        public void OnHotkeySet()
        {
            var saved = _settings.Read<WindowOptions>(WindowOptions.Key);
            if (saved == null || saved.VirtualKey == 0)
            {
                _hotkeyService?.Dispose();
                _hotkeyService = null;
                return;
            }

            _hotkeyService?.Dispose();

            _hotkeyService = new HotkeyService(this);

            var (mod, key) = Convert(saved);

            _hotkeyService.Register(mod, key);
            _hotkeyService.HotkeyPressed += ToggleVisibility;
        }

        private void ToggleVisibility()
        {
            if(this.WindowState == WindowState.Minimized)
            {
                this.WindowState = WindowState.Maximized;
                ShowInTaskbar = false;
                return;
            }
            if (Opacity == 0)
            {
                MonitorHelper.MoveWindowToActiveScreen(this);
                this.Show();
                Opacity = 1;
                IsHitTestVisible = true;
                Activate();
            }
            else
            {
                Opacity = 0;
                IsHitTestVisible = false;
                this.Hide();
            }
        }

        public void Minimize()
        {
            this.WindowState = WindowState.Minimized;
            this.ShowInTaskbar = true;
        }

        private (uint mod, uint key) Convert(WindowOptions cfg)
        {
            uint modifiers = 0;

            if (cfg.Ctrl) modifiers |= 0x0002;
            if (cfg.Alt) modifiers |= 0x0001;
            if (cfg.Shift) modifiers |= 0x0004;
            if (cfg.Win) modifiers |= 0x0008;

            return (modifiers, cfg.VirtualKey);
        }

        protected override void OnClosed(EventArgs e)
        {
            _hotkeyService?.Dispose();
            base.OnClosed(e);
        }
    }
}