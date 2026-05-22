using Cyrena;
using Cyrena.Contracts;
using Cyrena.Desktop.Models;
using Cyrena.Desktop.Services;
using Cyrena.Extensions;
using Cyrena.Host.Test.Components;
using Cyrena.Host.Test.Components.Shared;
using Cyrena.Options;
using Microsoft.Extensions.FileProviders;
using Photino.NET;

public partial class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledError;
        var appBuilder = WebApplication.CreateBuilder(args);

        using var cts = new CancellationTokenSource();
        if (!Directory.Exists(CyrenaBuilder.UserContentDirectory))
            Directory.CreateDirectory(CyrenaBuilder.UserContentDirectory);
        if (!Directory.Exists(CyrenaBuilder.ConversationsData))
            Directory.CreateDirectory(CyrenaBuilder.ConversationsData);

        var startup = Environment.GetFolderPath(Environment.SpecialFolder.Startup);

        appBuilder.Services
            .AddLogging(l =>
            {
#if DEBUG
                l.AddConsole();
#endif
            });

        appBuilder.Services.AddRazorComponents()
            .AddInteractiveServerComponents(options =>
            {
                options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromHours(8);
                options.DisconnectedCircuitMaxRetained = 500;
                options.JSInteropDefaultCallTimeout = TimeSpan.FromMinutes(5);
            });
        appBuilder.Services.AddSignalR(options =>
        {
            options.ClientTimeoutInterval = TimeSpan.FromMinutes(2);
            options.KeepAliveInterval = TimeSpan.FromSeconds(15);
            options.HandshakeTimeout = TimeSpan.FromSeconds(30);
            options.MaximumReceiveMessageSize = 10 * 1024 * 1024;
        });

        var port = appBuilder.Configuration.GetValue<int>("Port");
        appBuilder.WebHost.UseUrls($"http://localhost:{port}");

        var builder = appBuilder.Services.AddCyrenaRuntime()
                        .AddExtensa(e =>
                        {
                            e.ExtensionInfoFileName = "extension.json";
                            e.ExtensionsDirectory = Path.Combine(CyrenaBuilder.AppDataDirectory, "extensions");
                            e.InstallationsDirectory = Path.Combine(CyrenaBuilder.AppDataDirectory, "install");
                        })
                        .AddExtension<CyrenaExtension>(CyrenaExtension.Id, CyrenaExtension.Name, CyrenaExtension.Version, CyrenaExtension.Description);

        //Platform Specific Implementation
        var settings = builder.GetFeatureOption<ISettingsService>();
        var photino = settings.Read<WindowOptions>(WindowOptions.Key) ?? new WindowOptions();
        var window = new PhotinoWindow();
        window.SetIconFile("favicon.ico")
            .SetTitle("Cyréna")
            .Load(new Uri($"http://localhost:{port}"))
            .SetTransparent(false)
            .SetDevToolsEnabled(true)
            .SetContextMenuEnabled(true)
            .Center();
#if DEBUG
        window.SetDevToolsEnabled(true);
        window.SetContextMenuEnabled(true);
#else
                window.SetDevToolsEnabled(false);
                window.SetContextMenuEnabled(false);
#endif
        window.Height = photino.Height;
        window.Width = photino.Width;

        //window.WindowSizeChanged += OnWindowSizeChanged;
        builder.Services.AddSingleton(window);
        builder.Services.AddSingleton<IFileDialog, FileDialog>();
        builder.Services.AddSingleton<ISetupService, SetupService>();
        builder.AddSettingsComponent<Defaults>("Defaults");
        builder.Services.AddSingleton<IBrowserService, BrowserService>();
        //

        builder.Build();

        var app = appBuilder.Build();
        var fpu = new PhysicalFileProvider(CyrenaBuilder.UserContentDirectory);
        var fpc = new PhysicalFileProvider(CyrenaBuilder.ConversationsData);

        app.UseStaticFiles();
        app.UseStaticFiles(new StaticFileOptions()
        {
            FileProvider = fpu,
        });
        app.UseStaticFiles(new StaticFileOptions()
        {
            FileProvider = fpc,
        });
        app.UseAntiforgery();

        app.MapStaticAssets();
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode()
            .AddAdditionalAssemblies(builder.FeatureAssemblies["blazor"].ToArray());
        app.Run();
    }

    private static void OnUnhandledError(object sender, UnhandledExceptionEventArgs error)
    {
        var text = error.ExceptionObject?.ToString() ?? "Unknown crash";
        var path = $"./crash_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.log";

        try { File.WriteAllText(path, text); } catch { }
    }
}