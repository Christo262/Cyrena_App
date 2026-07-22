using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Options;
using Cyrena.Shell.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;

namespace Cyrena.Shell.Extensions
{
    public static class BackgroundApp
    {
        public static (WebApplication, string) CreateApp(string[] args, Cyrena.Shell.App ava)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.WebHost.UseShutdownTimeout(TimeSpan.Zero);
            builder.Services.AddSingleton(ava);
            builder.Services.AddLogging(l =>
            {
#if DEBUG
                l.AddConsole();
#endif
            });
            var cyrena = builder.Services.AddCyrenaRuntime()
                .AddExtensa(e =>
                {
                    e.ExtensionInfoFileName = "extension.json";
                    e.ExtensionsDirectory = Path.Combine(CyrenaBuilder.AppDataDirectory, "extensions");
                    e.InstallationsDirectory = Path.Combine(CyrenaBuilder.AppDataDirectory, "install");
                })
                .AddExtension<CyrenaExtension>(CyrenaExtension.Id, CyrenaExtension.Name, CyrenaExtension.Version, CyrenaExtension.Description);

            cyrena.Services.AddSingleton<ISetupService, SetupService>();
            cyrena.Services.AddSingleton<IFileDialog, FileDialog>();
            if(Environment.OSVersion.Platform == PlatformID.Unix)
                cyrena.Services.AddSingleton<IWindowLauncher, LinuxWindowService>();
            else
                cyrena.Services.AddSingleton<IWindowLauncher, DefaultWindowService>();
            cyrena.Build();
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents(options =>
                {
                    options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromHours(8);
                    options.DisconnectedCircuitMaxRetained = 500;
                    options.JSInteropDefaultCallTimeout = TimeSpan.FromMinutes(5);
                    options.DetailedErrors = true;
                });
            builder.Services.AddSignalR(options =>
            {
                options.ClientTimeoutInterval = TimeSpan.FromMinutes(2);
                options.KeepAliveInterval = TimeSpan.FromSeconds(15);
                options.HandshakeTimeout = TimeSpan.FromSeconds(30);
                options.MaximumReceiveMessageSize = 100 * 1024 * 1024;
            });
            var mvc =builder.Services.AddControllers();
            if(cyrena.FeatureAssemblies.ContainsKey("controllers"))
                foreach(var item in cyrena.FeatureAssemblies["controllers"])
                    mvc.AddApplicationPart(item);
            var settings = cyrena.GetFeatureOption<ISettingsService>();
            var appSettings = settings.Read<ApplicationOptions>(ApplicationOptions.Key) ?? new ApplicationOptions();
            var url = $"http://localhost:{appSettings.ServerPort}";
            builder.WebHost.UseUrls(url);
            var app = builder.Build();
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
            app.MapRazorComponents<Cyrena.Shell.Components.App>()
                .AddInteractiveServerRenderMode()
                .AddAdditionalAssemblies(cyrena.FeatureAssemblies["blazor"].ToArray());
            app.MapControllers();
            return (app, url);
        }
    }
}
