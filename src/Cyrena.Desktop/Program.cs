using Cyrena.Components.Shared;
using Cyrena.Contracts;
using Cyrena.Desktop.Components;
using Cyrena.Desktop.Components.Shared;
using Cyrena.Desktop.Models;
using Cyrena.Desktop.Services;
using Cyrena.Extensions;
using Cyrena.Options;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebView;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Photino.Blazor;
using Photino.NET;
using System.Runtime.InteropServices;
using System.Threading.Channels;

namespace Cyrena.Desktop;

class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        if (!Directory.Exists(CyrenaBuilder.UserContentDirectory))
            Directory.CreateDirectory(CyrenaBuilder.UserContentDirectory);
        var fpd = new PhysicalFileProvider(Path.Combine(AppContext.BaseDirectory, "wwwroot"));
        var fpu = new PhysicalFileProvider(CyrenaBuilder.UserContentDirectory);
        var appBuilder = PhotinoBlazorAppBuilder.CreateDefault(new CompositeFileProvider(fpd, fpu), args); //Photino/X Quirks
        appBuilder.Services
            .AddLogging(l =>
            {
#if DEBUG
                l.AddConsole();
#endif
            });

        appBuilder.RootComponents.Add<Cyrena.Components.Shared.HeadOutlet>("head-outlet");
        appBuilder.RootComponents.Add<App>("app");
        var builder = appBuilder.Services.AddCyrenaRuntime()
                        .AddExtensa(e =>
                        {
                            e.ExtensionInfoFileName = "extension.json";
                            e.ExtensionsDirectory = Path.Combine(CyrenaBuilder.AppDataDirectory, "extensions");
                            e.InstallationsDirectory = Path.Combine(CyrenaBuilder.AppDataDirectory, "install");
                        })
                        .AddExtension<CyrenaExtension>(CyrenaExtension.Id, CyrenaExtension.Name, CyrenaExtension.Version, CyrenaExtension.Description);

        //Platform Specific Implementation
        var files = new FileDialog();
        builder.Services.AddSingleton<IFileDialog>(files);  
        builder.Services.AddSingleton<ISetupService, SetupService>();
        //

        builder.AddSettingsComponent<Defaults>("Defaults");
        builder.Build();

        var app = appBuilder.Build();
        files.SetWindow(app.MainWindow);
        var settings = builder.GetFeatureOption<ISettingsService>();    
        var photino = settings.Read<WindowOptions>(WindowOptions.Key) ?? new WindowOptions();   
        app.MainWindow
            .SetIconFile("favicon.ico")
            .SetTitle("Cyréna")
            .Load("index.html")
            .Center();

#if DEBUG
        app.MainWindow.SetDevToolsEnabled(true);
#else
        app.MainWindow.SetDevToolsEnabled(false);
#endif

        app.MainWindow.Height = photino.Height;
        app.MainWindow.Width = photino.Width;

        app.MainWindow.WindowSizeChanged += (sender, args) =>
        {
            var m = settings.Read<WindowOptions>(WindowOptions.Key) ?? new WindowOptions();
            m.Height = args.Height;
            m.Width = args.Width;
            settings.Save(WindowOptions.Key, m);
        };

        AppDomain.CurrentDomain.UnhandledException += (sender, error) =>
        {
            var text = error.ExceptionObject?.ToString() ?? "Unknown crash";
            var path = $"./crash_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.log";

            try { File.WriteAllText(path, text); } catch { }

            try { app.MainWindow?.ShowMessage("Fatal exception", text); } catch { }
        };



        foreach (var item in builder.RunActions)
            item.Invoke(app.Services, builder.GetLifetimeCT());

        try
        {
            app.Run();
        }
        finally
        {
            builder.Dispose();
        }
    }
}
