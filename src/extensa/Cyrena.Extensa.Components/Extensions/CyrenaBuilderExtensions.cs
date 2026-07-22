using Cyrena.Extensa.Components;
using Cyrena.Extensa.Components.Shared;
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
                    Name = "Cyréna",
                    BaseUrl = "https://cyrena.dev",
                    ApplicationId = "ead9f857-5111-4759-a1f9-c75361e0a347",
                    Priority = 0,
                });

                if (ComponentOptions.IsServer)
                {
                    o.DefaultServers.Add(new DefaultServer()
                    {
                        Id = "vn-cyrena-code",
                        Name = "Cyréna Code",
                        BaseUrl = "https://cyrena.dev",
                        ApplicationId = "11486102-aa19-43d7-be0c-70aba7d9a51a",
                        Priority = 0,
                    });
                }
            });

            builder.Services.AddSingleton<IPluginServerService, PluginServerService>();
            builder.Services.AddSingleton<IPluginPackageService, PluginPackageService>();
            builder.Services.AddSingleton<IPackageManager, PackageManager>();
            builder.Services.AddSingleton<DownloadService>();
            builder.AddSettingsComponent<Servers>("Extensions", 10);

            builder.AddRunAction(async (sp, ct) =>
            {
                var dms = sp.GetRequiredService<DownloadService>();
                await dms.StartAsync(ct);
            });
            return builder;
        }
    }
}
