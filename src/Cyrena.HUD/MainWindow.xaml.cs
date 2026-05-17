using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.HUD.Components.Shared;
using Cyrena.HUD.Options;
using Cyrena.HUD.Services;
using Cyrena.Options;
using Microsoft.AspNetCore.Components.WebView;
using Microsoft.AspNetCore.Components.WebView.Wpf;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using System.IO;
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
        private readonly CyrenaBuilder _builder;
        private readonly CancellationTokenSource _cts;
        public MainWindow()
        {
            DataContext = this;
            _cts = new CancellationTokenSource();
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddWpfBlazorWebView();
            serviceCollection.AddBootstrapBlazor();
#if DEBUG
            serviceCollection.AddBlazorWebViewDeveloperTools();
#endif
            serviceCollection.AddSingleton(this);
            _builder = serviceCollection.AddCyrenaRuntime()
                                .AddExtensa(e =>
                                {
                                    e.ExtensionInfoFileName = "extension.json";
                                    e.ExtensionsDirectory = System.IO.Path.Combine(CyrenaBuilder.AppDataDirectory, "extensions");
                                    e.InstallationsDirectory = System.IO.Path.Combine(CyrenaBuilder.AppDataDirectory, "install");
                                })
                                .AddExtension<CyrenaExtension>(CyrenaExtension.Id, CyrenaExtension.Name, CyrenaExtension.Version, CyrenaExtension.Description);

            //Platform Specific Implementation
            var files = new FileDialog();
            _builder.Services.AddSingleton<IFileDialog>(files);
            _builder.Services.AddSingleton<ISetupService, SetupService>();
            //

            _builder.AddSettingsComponent<Defaults>("Defaults");
            _builder.Build();
            var sp = serviceCollection.BuildServiceProvider();
            _settings = sp.GetRequiredService<ISettingsService>();
            Resources.Add("services", sp);

            InitializeComponent();
            Loaded += MainWindow_Loaded;
            mainView.BlazorWebViewInitialized += OnBlazorWebViewInitialized;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= MainWindow_Loaded;
            var sp = (IServiceProvider)Resources["services"];
            foreach (var item in _builder.RunActions)
                item.Invoke(sp, _cts.Token);
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
            _cts.Cancel();
            _cts.Dispose();
            base.OnClosed(e);
        }
    }

    internal class CustomBlazorWebView : BlazorWebView
    {
        public override IFileProvider CreateFileProvider(string contentRootDir)
        {
            var defaultProvider = base.CreateFileProvider(contentRootDir);
            if(!Directory.Exists(CyrenaBuilder.UserContentDirectory))
                Directory.CreateDirectory(CyrenaBuilder.UserContentDirectory);
            var user = new PhysicalFileProvider(CyrenaBuilder.UserContentDirectory);
            return new CompositeFileProvider(defaultProvider, user);
        }
    }
}