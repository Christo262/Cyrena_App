using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cyrena.HUD.Options
{
    public static class ThemeHelper
    {
        public static bool IsDarkMode()
        {
            const string keyPath =
                @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";

            using var key = Registry.CurrentUser.OpenSubKey(keyPath);

            var value = key?.GetValue("AppsUseLightTheme");

            if (value is int i)
                return i == 0;

            return false;
        }
    }
}
