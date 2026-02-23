using System.Configuration;
using System.Data;
using System.Diagnostics;
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

            base.OnStartup(e);
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
