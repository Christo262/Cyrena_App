using Cyrena.Contracts;
using Cyrena.Models;
using Cyrena.Persistence.Options;

namespace Cyrena.Extensions
{
    public static class DeveloperContextBuilderExtensions
    {
        /// <summary>
        /// Adds a <see cref="IStore"/> to the <see cref="IDeveloperContextBuilder"/> services locked to the current project
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="builder"></param>
        /// <param name="collectionName"></param>
        /// <returns></returns>
        public static IDeveloperContextBuilder AddStore<TEntity>(this IDeveloperContextBuilder builder, string collectionName)
            where TEntity : class, IEntity
        {
            var p = builder.GetOption<ICyrenaPersistenceBuilder>();
            p!.AddSingletonStore<TEntity>(collectionName);
            return builder;
        }
    }
}
