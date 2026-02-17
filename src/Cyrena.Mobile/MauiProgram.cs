using Cyrena.Extensions;
using Cyrena.Mobile.Components.Shared;
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
