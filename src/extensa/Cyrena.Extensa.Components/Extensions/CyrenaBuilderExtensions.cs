using Cyrena.Extensa.Components;
using Cyrena.Extensa.Contracts;
using Cyrena.Extensa.Models;
using Cyrena.Extensa.Options;
using Cyrena.Extensa.Services;
using Cyrena.Options;
using Microsoft.Extensions.DependencyInjection;

namespace Cyrena.Extensions
{
    public static class CyrenaBuilderExtensions
    {
        public static CyrenaBuilder AddExtensaComponents(this CyrenaBuilder builder)
        {
            builder.AddFeatureAssembly<_Imports>("blazor");
            builder.AddSingletonStore<PluginServer>("extension_servers");
            builder.AddSingletonStore<Package>("extension_packages");
            builder.Services.Configure<PluginServerOptions>(o =>
            {
                o.DefaultServers.Add(new DefaultServer()
                {
                    Id = "vn-cyrena",
                    Name = "Vaya Nova Cyréna",
                    BaseUrl = "https://localhost:7000",
                    ApplicationId = "9756a97d-8bbb-4e2b-bcd1-bb95a53ab791",
                    Priority = 0,
                });
            });

            builder.Services.AddSingleton<IPluginServerService, PluginServerService>();
            builder.Services.AddSingleton<IPluginPackageService, PluginPackageService>();
            builder.Services.AddSingleton<IPackageManager, PackageManager>();
            builder.Services.AddSingleton<DownloadService>();

            builder.AddRunAction(async (sp, ct) =>
            {
                var dms = sp.GetRequiredService<DownloadService>();
                await dms.StartAsync(ct);
            });
            return builder;
        }
    }
}
