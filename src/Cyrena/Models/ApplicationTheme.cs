using MudBlazor;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cyrena.Models
{
    internal class ThemeChangePipeline : EventPipeline
    {
        public IDisposable Watch(Action cb) => this.ConfigurePipe("theme", cb);
        public void Invoke() => this.InvokePipeline("theme");
    }
    public static class ApplicationTheme
    {
        private readonly static ThemeChangePipeline _pipe = new();
        private static bool _darkMode { get; set; } = true;

        public static bool DarkMode
        {
            get => _darkMode;
            set
            {
                _darkMode = value;
                _pipe.Invoke();
            }
        }

        public static IDisposable WatchTheme(Action cb) => _pipe.Watch(cb);

        public static readonly PaletteLight Light = new()
        {
            Primary = "#E95420",
            PrimaryContrastText = "#FFFFFF",

            Secondary = "#77216F",
            SecondaryContrastText = "#FFFFFF",

            Tertiary = "#AEA79F",
            TertiaryContrastText = "#111111",

            Black = "#111111",

            Background = "#F7F5F3",
            BackgroundGray = "#EFEDEA",
            Surface = "#FFFFFF",

            AppbarText = "#2C001E",
            AppbarBackground = "rgba(255,255,255,0.86)",

            DrawerBackground = "#F7F5F3",
            DrawerText = "#2C001E",
            DrawerIcon = "#6E6259",

            TextPrimary = "#2C001E",
            TextSecondary = "#5E545C",
            TextDisabled = "#9B928C",

            ActionDefault = "#6E6259",
            ActionDisabled = "#AEA79F",
            ActionDisabledBackground = "#E6E1DD",

            GrayLight = "#DEDAD6",
            GrayLighter = "#F7F5F3",

            LinesDefault = "#DEDAD6",
            TableLines = "#E8E4E0",
            Divider = "#DEDAD6",

            Info = "#19B6EE",
            InfoContrastText = "#FFFFFF",

            Success = "#0E8420",
            SuccessContrastText = "#FFFFFF",

            Warning = "#E95420",
            WarningContrastText = "#FFFFFF",

            Error = "#C7162B",
            ErrorContrastText = "#FFFFFF",

            OverlayLight = "rgba(255,255,255,0.72)"
        };

        public static readonly PaletteDark Dark = new()
        {
            Primary = "#E95420",
            PrimaryContrastText = "#FFFFFF",

            Secondary = "#77216F",
            SecondaryContrastText = "#FFFFFF",

            Black = "#000000",

            // Proper neutral greys
            Background = "#1E1E1E",
            BackgroundGray = "#181818",
            Surface = "#242424",

            AppbarText = "#F2F2F2",
            AppbarBackground = "rgba(32,32,32,0.94)",

            DrawerBackground = "#181818",
            DrawerText = "#E6E6E6",
            DrawerIcon = "#A8A8A8",

            TextPrimary = "#F2F2F2",
            TextSecondary = "#C7C7C7",
            TextDisabled = "#777777",

            ActionDefault = "#B5B5B5",
            ActionDisabled = "#777777",
            ActionDisabledBackground = "#333333",

            GrayLight = "#333333",
            GrayLighter = "#242424",

            LinesDefault = "#383838",
            TableLines = "#303030",
            Divider = "#383838",

            Info = "#19B6EE",
            InfoContrastText = "#000000",

            Success = "#26A269",
            SuccessContrastText = "#FFFFFF",

            Warning = "#E95420",
            WarningContrastText = "#FFFFFF",

            Error = "#F66151",
            ErrorContrastText = "#FFFFFF",

            OverlayLight = "rgba(32,32,32,0.72)"
        };

        public static readonly MudTheme Theme = new()
        {
            PaletteLight = Light,
            PaletteDark = Dark,
            LayoutProperties = new LayoutProperties()
            {
                AppbarHeight = "45px"
            },
        };
    }
}
