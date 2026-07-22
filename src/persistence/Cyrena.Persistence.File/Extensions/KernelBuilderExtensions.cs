using Cyrena.Contracts;
using Cyrena.Models;
using Cyrena.Options;
using Cyrena.Persistence.File.Options;
using Cyrena.Persistence.Options;
using Cyrena.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

namespace Cyrena.Extensions
{
    public static class KernelBuilderExtensions
    {
        /// <summary>
        /// Adds file system storage scoped to the 'path' provided
        /// </summary>
        /// <param name="builder"><see cref="IKernelBuilder"/></param>
        /// <param name="path">Path to storage folder, i.e. {root_dir}/.cyrena</param>
        /// <param name="extension"></param>
        /// <returns><see cref="ICyrenaPersistenceBuilder"/></returns>
        public static ICyrenaPersistenceBuilder AddFilePersistence(this CyrenaKernelBuilder builder, string path, string extension = "json")
        {
            builder.Services.Configure<FilePersistenceOptions>(fs =>
            {
                fs.BaseDirectory = path;
                fs.FileExtension = extension;
            });
            builder.Services.AddSingleton<IPersistenceFS, PersistenceFS>();
            var p = new FilePersistenceBuilder(builder.Services);
            builder.AddFeatureOption<ICyrenaPersistenceBuilder>(p);
            return p;
        }

        /// <summary>
        /// Storage mechanism that is only loaded into memory while kernel instance is active.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="path"></param>
        /// <param name="configure"></param>
        /// <param name="extension"></param>
        public static void AddIsolatedFilePersistence(this CyrenaKernelBuilder builder, string path, Action<ICyrenaPersistenceBuilder> configure, string extension = "json")
        {
            var options = new FilePersistenceOptions()
            {
                BaseDirectory = path,
                FileExtension = extension
            };
            var persistence = new IsolatedFilePersistenceBuilder(builder.Services, options);
            configure(persistence);
        }
    }
}
