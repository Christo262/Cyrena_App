using Cyrena.Contracts;
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
        public static ICyrenaPersistenceBuilder AddFilePersistence(this IKernelBuilder builder, string path, string extension = "json")
        {
            builder.Services.Configure<FilePersistenceOptions>(fs =>
            {
                fs.BaseDirectory = path;
                fs.FileExtension = extension;
            });
            builder.Services.AddSingleton<IPersistenceFS, PersistenceFS>();
            var p = new FilePersistenceBuilder(builder.Services);
            return p;
        }
    }
}
