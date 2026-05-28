using Cyrena.Android.Services;
using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Options;
using Cyrena.Runtime.Ollama.Services;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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
            if (!Directory.Exists(CyrenaBuilder.UserContentDirectory))
                Directory.CreateDirectory(CyrenaBuilder.UserContentDirectory);
            if (!Directory.Exists(CyrenaBuilder.ConversationsData))
                Directory.CreateDirectory(CyrenaBuilder.ConversationsData);
            builder.Services.AddSingleton<IFileProvider>(sp =>
            {
                var fpu = new PhysicalFileProvider(CyrenaBuilder.UserContentDirectory);
                var fpc = new PhysicalFileProvider(CyrenaBuilder.ConversationsData);
                return new CompositeFileProvider(fpu, fpc);
            });
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
            return app;
        }
    }
}
