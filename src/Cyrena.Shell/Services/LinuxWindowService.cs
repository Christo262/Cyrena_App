using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cyrena.Shell.Services
{
    public class LinuxWindowService : IWindowLauncher
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

        public void Show(string url, int width, int height, string title = "Cyréna")
        {
            var webViewDirectory = Path.Combine(
                AppContext.BaseDirectory,
                "webview");

            var path = Path.Combine(
                webViewDirectory,
                "Cyrena.WebView");

            if (!File.Exists(path))
                return;

            var info = new ProcessStartInfo
            {
                FileName = path,
                WorkingDirectory = webViewDirectory,
                UseShellExecute = false
            };

            info.ArgumentList.Add("--title");
            info.ArgumentList.Add(title);

            info.ArgumentList.Add("--url");
            info.ArgumentList.Add(url);

            info.ArgumentList.Add("--width");
            info.ArgumentList.Add(width.ToString());

            info.ArgumentList.Add("--height");
            info.ArgumentList.Add(height.ToString());

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
            UpdatePhotinoSize();
        }

        private void UpdatePhotinoSize()
        {
            var webViewDirectory = Path.Combine(
                AppContext.BaseDirectory,
                "webview");
            var file = Path.Combine(webViewDirectory, "photino.json");
            if (!File.Exists(file))
                return;
            try
            {
                var json = File.ReadAllText(file);
                var win = JsonSerializer.Deserialize<PhotinoWindowSize>(json);
                if (win == null || win.Width <= 0 || win.Height <= 0)
                    return;
                var settings = CyrenaRuntime.CreateSettings();
                var options = settings.Read<ApplicationOptions>(ApplicationOptions.Key) ?? new ApplicationOptions();
                options.Width = win.Width;
                options.Height = win.Height;
                settings.Save(ApplicationOptions.Key, options);
            }
            catch
            {
                return;
            }
        }
    }

    internal class PhotinoWindowSize
    {
        [JsonPropertyName("width")]
        public int Width { get; set; }
        [JsonPropertyName("height")]
        public int Height { get; set; }
    }
}