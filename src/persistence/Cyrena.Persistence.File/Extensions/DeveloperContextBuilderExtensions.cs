using Cyrena.Contracts;
using Cyrena.Models;
using Cyrena.Options;
using Cyrena.Persistence.File.Options;
using Cyrena.Persistence.Options;
using Cyrena.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Cyrena.Extensions
{
    public static class DeveloperContextBuilderExtensions
    {
        public static IDeveloperContextBuilder UseFilePersistence(this IDeveloperContextBuilder builder)
        {
            var p = new FilePersistenceBuilder(builder.Services);
            builder.AddOption<ICyrenaPersistenceBuilder>(p);
            builder.Services.AddSingleton<IPersistenceFS, PersistenceFS>();
            builder.Services.Configure<FilePersistenceOptions>(fs =>
            {
                fs.BaseDirectory = Path.Combine(builder.Project.RootDirectory, Project.CyrenaDirectory);
                fs.FileExtension = "json";
            });
            return builder;
        }
    }
}
