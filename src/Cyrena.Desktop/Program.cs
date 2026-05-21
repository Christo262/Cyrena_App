using Cyrena.Contracts;
using Cyrena.Desktop.Components;
using Cyrena.Desktop.Components.Shared;
using Cyrena.Desktop.Models;
using Cyrena.Desktop.Services;
using Cyrena.Extensions;
using Cyrena.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Photino.Blazor;

namespace Cyrena.Desktop;

class Program
{
    private static PhotinoBlazorApp? _app { get; set; }
    [STAThread]
    static void Main(string[] args)
    {
        using var cts = new CancellationTokenSource();
        if (!Directory.Exists(CyrenaBuilder.UserContentDirectory))
            Directory.CreateDirectory(CyrenaBuilder.UserContentDirectory);
        var fpd = new PhysicalFileProvider(Path.Combine(AppContext.BaseDirectory, "wwwroot"));
        var fpu = new PhysicalFileProvider(CyrenaBuilder.UserContentDirectory);
        var appBuilder = PhotinoBlazorAppBuilder.CreateDefault(new CompositeFileProvider(fpd, fpu), args);
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
        builder.Services.AddSingleton<IFileDialog, FileDialog>();  
        builder.Services.AddSingleton<ISetupService, SetupService>();
        builder.AddSettingsComponent<Defaults>("Defaults");
        //

        builder.Build();

        _app = appBuilder.Build();
        var settings = builder.GetFeatureOption<ISettingsService>();    
        var photino = settings.Read<WindowOptions>(WindowOptions.Key) ?? new WindowOptions();
        _app.MainWindow
            .SetIconFile("favicon.ico")
            .SetTitle("Cyréna")
            .Load("index.html")
            .Center();
#if DEBUG
        _app.MainWindow.SetDevToolsEnabled(true);
#else
        _app.MainWindow.SetDevToolsEnabled(false);
#endif
        _app.MainWindow.Height = photino.Height;
        _app.MainWindow.Width = photino.Width;

        _app.MainWindow.WindowSizeChanged += OnWindowSizeChanged;

        AppDomain.CurrentDomain.UnhandledException += OnUnhandledError;

        foreach (var item in builder.RunActions)
            item.Invoke(_app.Services, cts.Token);
        _app.Run();
    }

    private static void OnUnhandledError(object sender, UnhandledExceptionEventArgs error)
    {
        var text = error.ExceptionObject?.ToString() ?? "Unknown crash";
        var path = $"./crash_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.log";

        try { File.WriteAllText(path, text); } catch { }
        try { _app?.MainWindow?.ShowMessage("Fatal exception", text); } catch { }
    }

    private static void OnWindowSizeChanged(object? sender, System.Drawing.Size args)
    {
        if (_app == null) return;
        var settings = _app.Services.GetRequiredService<ISettingsService>();
        var m = settings.Read<WindowOptions>(WindowOptions.Key) ?? new WindowOptions();
        m.Height = args.Height;
        m.Width = args.Width;
        settings.Save(WindowOptions.Key, m);
    }
}
