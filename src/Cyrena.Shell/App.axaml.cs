using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Cyrena.Contracts;
using Cyrena.Options;
using Cyrena.Shell.Extensions;
using Cyrena.Shell.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
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
        private MainWindow? _mainWindow;
        private SplashWindow? _splashWindow;

        public ICommand OpenShell { get; }
        public ICommand ExitApp { get; }

        public App()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            OpenShell = new DelegateCommand(ShowWindow);
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

                Task.Run(async () =>
                {
                    try
                    {
                        (_background, string url) = BackgroundApp.CreateApp([]);
                        await _background.StartAsync(_backgroundToken.Token);

                        using var http = new HttpClient();
                        while (true)
                        {
                            try
                            {
                                var res = await http.GetAsync(url);
                                if (res.IsSuccessStatusCode) break;
                            }
                            catch { }
                            await Task.Delay(200);
                        }
                    }
                    catch (Exception ex)
                    {
                        
                    }
                }, _backgroundToken.Token).ContinueWith(t =>
                {
                    Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                    {
                        _splashWindow.Close();
                        if (t.IsFaulted)
                        {
                            //TODO
                            return;
                        }
                        _mainWindow = new MainWindow();
                        _mainWindow.ShowInTaskbar = false;
                        var fd = _background!.Services.GetRequiredService<IFileDialog>() as FileDialog;
                        fd!.SetWindow(_mainWindow);
                        var options = _background.Services.GetRequiredService<ISettingsService>().Read<ApplicationOptions>(ApplicationOptions.Key) ?? new ApplicationOptions();
                        if (options.LaunchWindowOnStartup == true)
                            ShowWindow();
                    });                   
                });
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void ShowWindow()
        {
            if (!Avalonia.Threading.Dispatcher.UIThread.CheckAccess())
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(ShowWindow);
                return;
            }

            if (_mainWindow is null) return;

            _mainWindow.ShowInTaskbar = true;
            _mainWindow.Show();
            _mainWindow.WindowState = WindowState.Normal;
            _mainWindow.Activate();
        }

        private void Exit()
        {
            if (_background != null)
            {
                _background.StopAsync(CancellationToken.None)
                   .Wait(TimeSpan.FromMilliseconds(500));
                _background.DisposeAsync().GetAwaiter().GetResult();
            }
            _backgroundToken.Cancel();
            _backgroundToken.Dispose();

            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                desktop.Shutdown();
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