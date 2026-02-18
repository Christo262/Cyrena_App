using Cyrena.Contracts;
using Cyrena.Desktop.Components;
using Cyrena.Desktop.Components.Shared;
using Cyrena.Desktop.Models;
using Cyrena.Desktop.Services;
using Cyrena.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Photino.Blazor;

namespace Cyrena.Desktop;

class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        var appBuilder = PhotinoBlazorAppBuilder.CreateDefault(args);
        appBuilder.Services
            .AddLogging();

        appBuilder.RootComponents.Add<App>("app");
        var builder = appBuilder.Services.AddCyrenaRuntime()
            .AddComponents()
            .AddOllama()
            .AddOpenAI()
            .AddTavily()
            .AddApiReferencePages()
            .AddDeveloperRuntime()
            .AddDotnetDevelopment()
            .AddPlatformIO()
            .AddArduinoIDE();
        var files = new FileDialog();
        builder.Services.AddSingleton<IFileDialog>(files);  
        builder.AddSettingsComponent<Defaults>();
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

        app.Run();
    }
}