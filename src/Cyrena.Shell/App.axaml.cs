using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Options;
using Cyrena.Shell.Extensions;
using Cyrena.Shell.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Cyrena.Shell
{
    public partial class App : Application
    {
        private readonly CancellationTokenSource _backgroundToken;
        private WebApplication? _background;
        private SplashWindow? _splashWindow;

        public ICommand OpenShell { get; }
        public ICommand OpenBrowser { get; }
        public ICommand ExitApp { get; }

        public App()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            OpenShell = new DelegateCommand(ShowWindow);
            OpenBrowser = new DelegateCommand(OpenWebBrowser);
            ExitApp = new DelegateCommand(Exit);
            _backgroundToken = new CancellationTokenSource();
        }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
            if (!Directory.Exists(CyrenaBuilder.UserContentDirectory))
                Directory.CreateDirectory(CyrenaBuilder.UserContentDirectory);
            if (!Directory.Exists(CyrenaBuilder.ConversationsData))
                Directory.CreateDirectory(CyrenaBuilder.ConversationsData);
            DataContext = this;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs error)
        {
            var text = error.ExceptionObject?.ToString() ?? "Unknown crash";
            var path = $"./crash_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.log";

            try { System.IO.File.WriteAllText(path, text); } catch { }
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;

                _splashWindow = new SplashWindow();
                _splashWindow.Show();
                Exception? error = null;
                string url = string.Empty;
                Task.Run(async () =>
                {
                    try
                    {
                        (_background, url) = BackgroundApp.CreateApp([], this);
                        using var http = new HttpClient()
                        {
                            BaseAddress = new Uri(url)
                        };
                        try
                        {
                            using var check = await http.GetAsync("/api/is-alive");
                            if (check.IsSuccessStatusCode)
                            {
                                if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                                    desktop.Shutdown();
                                return;
                            }

                        }
                        catch { }
                        await _background.StartAsync(_backgroundToken.Token);

                        while (true)
                        {
                            try
                            {
                                using var res = await http.GetAsync(url);
                                if (res.IsSuccessStatusCode) break;
                            }
                            catch { }
                            await Task.Delay(200);
                        }
                    }
                    catch (Exception ex)
                    {
                        error = ex;
                    }
                }, _backgroundToken.Token).ContinueWith(t =>
                {
                    Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                    {
                        _splashWindow.Hide(); //Need it for file dialog access
                        if (t.IsFaulted || _background == null || error != null)
                        {
                            var fail = new ErrorWindow(error?.Message ?? "Failed to start background services");
                            fail.Show();
                            return;
                        }
                        var fd = _background.Services.GetRequiredService<IFileDialog>();
                        if (fd is FileDialog nfd)
                            nfd.SetWindow(_splashWindow);
                        var options = CyrenaRuntime.CreateSettings().Read<ApplicationOptions>(ApplicationOptions.Key) ?? new ApplicationOptions();
                        if (options.LaunchWindowOnStartup == true)
                            ShowWindow();
                    });                   
                });
            }

            base.OnFrameworkInitializationCompleted();
        }

        public void ShowWindow()
        {
            if(_background != null)
            {
                var options = CyrenaRuntime.CreateSettings().Read<ApplicationOptions>(ApplicationOptions.Key) ?? new ApplicationOptions();
                var windows = _background.Services.GetRequiredService<IWindowLauncher>();
                windows.ShowMain(options);
            }
        }

        private void Exit()
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                desktop.Shutdown();
            if (_background != null)
            {
                _background.StopAsync(CancellationToken.None)
                   .Wait(TimeSpan.FromMilliseconds(500));
                _background.DisposeAsync();
            }
            _backgroundToken.Cancel();
            _backgroundToken.Dispose();
        }

        private void OpenWebBrowser()
        {
            var options = CyrenaRuntime.CreateSettings().Read<ApplicationOptions>(ApplicationOptions.Key) ?? new ApplicationOptions();
            var url = $"http://localhost:{options.ServerPort}";
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
    }

    internal class DelegateCommand : ICommand
    {
        private readonly Action _execute;
        public DelegateCommand(Action execute) => _execute = execute;
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) => _execute();
        public event EventHandler? CanExecuteChanged;
    }
}