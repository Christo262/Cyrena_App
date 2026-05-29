using Cyrena.Contracts;
using Cyrena.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cyrena.Extensions
{
    public static class WindowLauncherExtensions
    {
        public static void ShowMain(this IWindowLauncher windows, ApplicationOptions options)
        {
            windows.Show($"http://localhost:{options.ServerPort}", options.Width, options.Height);
        }
    }
}
