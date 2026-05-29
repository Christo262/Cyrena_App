#if ANDROID
using AndroidApp = Android.App.Application;
using AndroidContext = Android.Content.Context;
using Cyrena.Platforms;
using AndroidIntent = Android.Content.Intent;
using Cyrena.Platforms.Android;
using AndroidW = Android.Webkit;
#endif
using Cyrena.Android.Services;
using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Options;
using Cyrena.Runtime.Ollama.Services;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;


namespace Cyrena.Android
{
    public static class MauiProgram
    {
        private static CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });
#if ANDROID
            builder.ConfigureLifecycleEvents(events =>
             {
                 events.AddAndroid(android => android.OnCreate((activity, bundle) =>
                 {
                     AndroidW.WebView.SetWebContentsDebuggingEnabled(true);
                 }));
             });
#endif
            var cyrena = builder.Services.AddCyrenaRuntime()
               .AddExtensa(e =>
               {
                   e.ExtensionInfoFileName = "extension.json";
                   e.ExtensionsDirectory = Path.Combine(CyrenaBuilder.AppDataDirectory, "extensions");
                   e.InstallationsDirectory = Path.Combine(CyrenaBuilder.AppDataDirectory, "install");
               })
               .AddExtension<CyrenaExtension>(CyrenaExtension.Id, CyrenaExtension.Name, CyrenaExtension.Version, CyrenaExtension.Description);

            cyrena.Services.AddSingleton<IFileDialog, FileDialog>();
            cyrena.Services.AddSingleton<IWindowLauncher, WindowLauncher>();
            cyrena.Services.AddSingleton<ISetupService, SetupService>();
            cyrena.AddAssistantPlugin<AndroidAssistansPlugin>();
#if ANDROID
            cyrena.Services.AddSingleton<Cyrena.Platforms.Android.KernelOrchestrator>();
            builder.Services.AddSingleton<AndroidContext>(_ => AndroidApp.Context);
            cyrena.AddRunAction((sp, ct) =>
            {
                var str = sp.GetRequiredService<KernelOrchestrator>();
                str.RunAsync(ct);
            });
#endif
            builder.Services.AddMauiBlazorWebView();

#if DEBUG
    		builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif
            var ui = cyrena.GetFeatureOption<ComponentOptions>();
            ui.MenuOptions.AllowNewTab = false;
            ui.MenuOptions.ConverseUrl = "converse-maui/{Id}";
            ui.FileSystemOptions.ExploreFolder = false;
            ComponentOptions.OllamaDefaultEndpoint = "https://ollama.com";
            ComponentOptions.IsServer = false;
            cyrena.Build();
            var app = builder.Build();
            foreach (var item in cyrena.RunActions)
                item.Invoke(app.Services, _cancellationTokenSource.Token);

#if ANDROID
            MauiAppContainer.Provider = app.Services;
#endif
            return app;
        }
    }
}
