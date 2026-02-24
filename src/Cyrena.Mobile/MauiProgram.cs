using CommunityToolkit.Maui;
using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Mobile.Components.Shared;
using Cyrena.Mobile.Services;
using Microsoft.Extensions.Logging;

namespace Cyrena.Mobile
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();
            var cyrena = builder.Services.AddCyrenaRuntime()
               .AddComponents()
               .AddOllama()
               .AddOpenAI()
               .AddTavily();
            builder.Services.AddSingleton<IFileDialog, FileDialog>();
            cyrena.AddSettingsComponent<Defaults>();
            cyrena.Build();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
