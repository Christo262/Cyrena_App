using Cyrena.Contracts;
using Cyrena.Desktop.Components;
using Cyrena.Desktop.Services;
using Cyrena.Extensions;
using Cyrena.Options;
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

        var cyrena = CyrenaBuilder.Create(appBuilder.Services)
            .UseFilePersistence(fs =>
            {
                fs.BaseDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ".cyrena");
                fs.FileExtension = "json";
            })
            .AddRuntime(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ".cyrena"))
            .AddComponents()
            .AddOllama()
            .AddOpenAI()
            .AddSpecifications()
            .AddTavily()
            .AddBlazorDevelopment()
            .AddClassLibraryDevelopment()
            .AddPlatformIO()
            .AddArduinoIDE();

        var settings = cyrena.GetOption<ISettingsService>();
        var winCurr = new CurrentWindow(settings);
        cyrena.Services.AddSingleton<ICurrentWindow>(winCurr);
        cyrena.Build();

        appBuilder.RootComponents.Add<App>("app");
        appBuilder.Services.Configure<App>(a =>
        {

        });
        var app = appBuilder.Build();

        // customize window
        app.MainWindow
            .SetIconFile("favicon.ico")
            .SetTitle("Cyréna")
            .Load("index.html")
            .Center();

        app.MainWindow.Height = 740;
        app.MainWindow.Width = 1000;
        winCurr.SetWindow(app.MainWindow);

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