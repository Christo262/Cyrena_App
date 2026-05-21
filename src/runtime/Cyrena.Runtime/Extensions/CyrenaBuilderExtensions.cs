using Cyrena.Contracts;
using Cyrena.Models;
using Cyrena.Options;
using Cyrena.Runtime.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Cyrena.Extensions
{
    public static class CyrenaBuilderExtensions
    {
        public static CyrenaBuilder AddCyrenaRuntime(this IServiceCollection services)
        {
            var builder = new CyrenaBuilder(services);
            builder.UseFilePersistence(fs =>
            {
                fs.BaseDirectory = Path.Combine(CyrenaBuilder.AppDataDirectory, "app-data");
                fs.FileExtension = "json";
            });
            var settings = new SettingsService(CyrenaBuilder.AppDataDirectory);
            builder.Services.AddSingleton<ISettingsService>(settings);
            builder.AddFeatureOption<ISettingsService>(settings);
            builder.AddSingletonStore<ChatConfiguration>("chats");

            builder.Services.AddSingleton<IKernelController, KernelController>();
            builder.AddAssistantPlugin<AllAssistantsPlugin>();

            return builder;
        }
    }
}
