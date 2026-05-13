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

        //#if DEBUG
        //        app.MainWindow.SetDevToolsEnabled(true);
        //#else
        //        app.MainWindow.SetDevToolsEnabled(false);
        //#endif
        app.MainWindow.SetDevToolsEnabled(true);

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

        app.Run();
    }

    internal class CustomWebViewManager : WebViewManager
    {
        private readonly PhotinoWindow _window;
        private readonly Channel<string> _channel;
        private readonly SynchronousTaskScheduler _sts;
        private readonly CancellationTokenSource _cts = new();

        // On Windows, we can't use a custom scheme to host the initial HTML,
        // because webview2 won't let you do top-level navigation to such a URL.
        // On Linux/Mac, we must use a custom scheme, because their webviews
        // don't have a way to intercept http:// scheme requests.
        public static readonly string BlazorAppScheme = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? "http"
            : "app";

        public static readonly Uri AppBaseUri = new($"{BlazorAppScheme}://localhost/");

        public CustomWebViewManager(PhotinoWindow window, IServiceProvider provider, Dispatcher dispatcher,
            IFileProvider fileProvider, JSComponentConfigurationStore jsComponents, IOptions<PhotinoBlazorAppConfiguration> config)
            : base(provider, dispatcher, config.Value.AppBaseUri, fileProvider, jsComponents, config.Value.HostPage)
        {
            _window = window ?? throw new ArgumentNullException(nameof(window));

            //Create channel and start reader
            _channel = Channel.CreateUnbounded<string>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false,
                AllowSynchronousContinuations = false
            });

            // Create a scheduler that uses one threads.
            _sts = new SynchronousTaskScheduler();

            _window.WebMessageReceived += WebMessageReceived;

            _ = Task.Run(() => MessagePumpAsync(_cts.Token), _cts.Token);
        }

        private void WebMessageReceived(object? sender, string message)
        {
            // On some platforms, we need to move off the browser UI thread
            _ = Task.Factory.StartNew(static state =>
            {
                (CustomWebViewManager wvm, string message) = ((CustomWebViewManager, string))state!;
                // TODO: Fix this. Photino should ideally tell us the URL that the message comes from so we
                // know whether to trust it. Currently, it's hardcoded to trust messages from any source, including
                // if the webview is somehow navigated to an external URL.
                var messageOriginUrl = AppBaseUri;

                wvm.MessageReceived(messageOriginUrl, message);
            }, (this, message), _cts.Token, TaskCreationOptions.DenyChildAttach, _sts);
        }

        public Stream? HandleWebRequest(object? sender, string schema, string url, out string? contentType)
        {
            _ = sender;
            _ = schema;
            // It would be better if we were told whether this is a navigation request, but
            // since we're not, guess.
            var localPath = (new Uri(url)).LocalPath;
            var hasFileExtension = localPath.LastIndexOf('.') > localPath.LastIndexOf('/');

            //Remove parameters before attempting to retrieve the file. For example: http://localhost/_content/Blazorise/button.js?v=1.0.7.0
            if (url.Contains('?')) url = url.Substring(0, url.IndexOf('?'));

            if (url.StartsWith(AppBaseUri.ToString(), StringComparison.Ordinal)
                && TryGetResponseContent(url, !hasFileExtension, out _, out _,
                    out var content, out var headers))
            {
                headers.TryGetValue("Content-Type", out contentType);
                return content;
            }

            contentType = null;
            return null;
        }

        protected override void NavigateCore(Uri absoluteUri)
        {
            _window.Load(absoluteUri);
        }

        protected override void SendMessage(string message)
        {
            _ = _channel.Writer.WriteAsync(message, _cts.Token);
        }

        private async Task MessagePumpAsync(CancellationToken cancellationToken)
        {
            var reader = _channel.Reader;
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var message = await reader.ReadAsync(cancellationToken).ConfigureAwait(false);
                    _window.SendWebMessage(message);
                }
            }
            catch (OperationCanceledException) { }
            catch (ChannelClosedException) { }
        }

        protected override ValueTask DisposeAsyncCore()
        {
            try { _cts.Cancel(); }
            catch { /* ignored */ }

            _window.WebMessageReceived -= WebMessageReceived;

            //complete channel
            try { _channel.Writer.Complete(); }
            catch (Exception ex)
            {
                //_window.Log($"Error completing channel: {ex}");
            }

            _cts.Dispose();

            //continue disposing
            return base.DisposeAsyncCore();
        }
    }

    class SynchronousTaskScheduler : TaskScheduler
    {
        public override int MaximumConcurrencyLevel
        {
            get { return 1; }
        }

        protected override void QueueTask(Task task)
        {
            TryExecuteTask(task);
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            return TryExecuteTask(task);
        }

        protected override IEnumerable<Task> GetScheduledTasks()
        {
            return Enumerable.Empty<Task>();
        }
    }
}
