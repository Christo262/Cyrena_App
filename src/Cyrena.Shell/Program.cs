using Avalonia;
using Cyrena.CLI.Extensions;
using Cyrena.CLI.Services;
using System;
using System.Runtime.InteropServices;
using System.Linq;

namespace Cyrena.Shell
{
    internal class Program
    {
        [STAThread]
        public static int Main(string[] args)
        {
            var app = BuildAvaloniaApp();

            if (args.Any())
            {
                ConsoleHelper.EnsureConsoleForCli(args);
                var cli = CliServiceExtensions.Create();
                cli.RegisterFromAssembly(typeof(App).Assembly);

                var result = cli.ParseAndExecute(args);
                if (result.ShouldContinueBoot)
                {
                    ConsoleHelper.HideConsoleAfterBoot();
                    return app.StartWithClassicDesktopLifetime(args);
                }
                if (!string.IsNullOrEmpty(result.Message))
                    Console.WriteLine(result.Message);

                if(OperatingSystem.IsWindows()) //Prevent just closing window and not showing output
                {
                    Console.WriteLine("Press any key to exit");
                    var key = Console.ReadKey();
                }
                return result.ExitCode ?? -1;
            }
            return app.StartWithClassicDesktopLifetime(args);
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
#if DEBUG
                .WithDeveloperTools()
#endif
                .WithInterFont()
                .LogToTrace();
    }

public static class ConsoleHelper
    {
        private static bool _consoleAllocated = false;

        public static void EnsureConsoleForCli(string[] args)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Linux/macOS: console already available if run from terminal
                return;
            }

            if (args.Length == 0)
            {
                // No CLI args, running as background service - no console needed
                return;
            }

            AllocConsole();
            _consoleAllocated = true;
        }

        public static void HideConsoleAfterBoot()
        {
            if (!_consoleAllocated || !RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return;
            }

            var handle = GetConsoleWindow();
            ShowWindow(handle, SW_HIDE);
        }

        #region Windows P/Invoke
        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_HIDE = 0;
        #endregion
    }

}
