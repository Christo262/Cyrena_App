using Cyrena.Options;
using Cyrena.Shell.Contracts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Cyrena.Shell.Services
{
    public class LinuxWindowService : IWindowService
    {
        private readonly object _lock = new();
        private readonly List<Process> _windows = new();

        public void Dispose()
        {
            List<Process> processes;

            lock (_lock)
            {
                processes = _windows.ToList();
                _windows.Clear();
            }

            foreach (var process in processes)
            {
                try
                {
                    if (!process.HasExited)
                    {
                        process.Kill(entireProcessTree: true);
                        process.WaitForExit(3000);
                    }
                }
                catch
                {
                    // Ignore shutdown cleanup errors.
                }
                finally
                {
                    process.Dispose();
                }
            }
        }

        public void Show(ApplicationOptions options)
        {
            var webViewDirectory = Path.Combine(
                AppContext.BaseDirectory,
                "webview");

            var path = Path.Combine(
                webViewDirectory,
                "Cyrena.WebView");

            if (!File.Exists(path))
                throw new Exception("Unable to find Cyréna WebView executable.");

            var info = new ProcessStartInfo
            {
                FileName = path,
                WorkingDirectory = webViewDirectory,
                UseShellExecute = false
            };

            info.ArgumentList.Add("--title");
            info.ArgumentList.Add("Cyréna");

            info.ArgumentList.Add("--url");
            info.ArgumentList.Add($"http://localhost:{options.ServerPort}");

            info.ArgumentList.Add("--width");
            info.ArgumentList.Add(options.Width.ToString());

            info.ArgumentList.Add("--height");
            info.ArgumentList.Add(options.Height.ToString());

            var process = new Process
            {
                StartInfo = info,
                EnableRaisingEvents = true
            };

            process.Exited += Process_Exited;

            try
            {
                process.Start();

                lock (_lock)
                {
                    _windows.Add(process);
                }
            }
            catch
            {
                process.Dispose();
                throw;
            }
        }

        private void Process_Exited(object? sender, EventArgs e)
        {
            if (sender is not Process process)
                return;

            lock (_lock)
            {
                _windows.Remove(process);
            }

            process.Exited -= Process_Exited;
            process.Dispose();
        }
    }
}