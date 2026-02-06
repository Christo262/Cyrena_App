using Microsoft.Extensions.DependencyInjection;
using Cyrena.Contracts;
using Cyrena.Models;
using Cyrena.Options;
using Cyrena.Runtime.Models;
using Cyrena.Runtime.Services;

namespace Cyrena.Extensions
{
    public static class CyrenaBuilderExtensions
    {
        public static CyrenaBuilder AddRuntime(this CyrenaBuilder builder, string settingsDir)
        {
            builder.AddScopedStore<Project>("projects");
            builder.AddScopedStore<Note>("project_notes");

            builder.Services.AddScoped<IProjectLoader, ProjectLoader>();
            builder.Services.AddScoped<IDeveloperContextExtension, DefaultBuilderExtension>();

            var settings = new SettingsService(settingsDir);
            builder.Services.AddSingleton<ISettingsService>(settings);
            builder.AddOption<ISettingsService>("Cyrena.settings", settings);

            return builder;
        }
    }
}
