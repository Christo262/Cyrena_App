using Cyrena.Contracts;
using Cyrena.Models;
using Cyrena.Options;
using Cyrena.Runtime.Models;
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
                fs.BaseDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ".cyrena");
                fs.FileExtension = "json";
            });
            var settings = new SettingsService(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ".cyrena"));
            builder.Services.AddSingleton<ISettingsService>(settings);
            builder.AddFeatureOption<ISettingsService>(settings);

            builder.AddSingletonStore<ChatMessage>("chat_messages");
            builder.AddSingletonStore<ChatConfiguration>("chats");

            builder.Services.AddSingleton<IKernelController, KernelController>();

            builder.AddAssistantMode<DefaultAssistantMode>();
            builder.AddAssistantPlugin<DefaultAssistantPlugin>();
            builder.AddAssistantPlugin<AllAssistantsPlugin>();

            return builder;
        }
    }
}
