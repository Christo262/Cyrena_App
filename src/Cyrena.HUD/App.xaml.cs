using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;

namespace Cyrena.HUD
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Mutex? _mutex;

        protected override void OnStartup(StartupEventArgs e)
        {
            const string appName = "cyrena.hud";
            bool createdNew;

            _mutex = new Mutex(true, appName, out createdNew);

            if (!createdNew)
            {
                if (!createdNew)
                {
                    // Find and bring the existing instance to the foreground
                    var current = Process.GetCurrentProcess();
                    foreach (var process in Process.GetProcessesByName(current.ProcessName))
                    {
                        if (process.Id != current.Id)
                        {
                            NativeMethods.SetForegroundWindow(process.MainWindowHandle);
                            break;
                        }
                    }
                    Application.Current.Shutdown();
                    return;
                }
            }
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            base.OnStartup(e);
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            var text = e.Exception.ToString() ?? "Unknown crash";
            var path = $"./crash_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.log";
            try { File.WriteAllText(path, text); } catch { }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                _mutex?.ReleaseMutex();
            }
            catch (ApplicationException)
            {
                // Mutex was not owned by this thread, safe to ignore
            }
            finally
            {
                _mutex?.Dispose();
            }
            base.OnExit(e);
        }
    }

    internal static class NativeMethods
    {
        [DllImport("user32.dll")]
        internal static extern bool SetForegroundWindow(IntPtr hWnd);
    }
}
