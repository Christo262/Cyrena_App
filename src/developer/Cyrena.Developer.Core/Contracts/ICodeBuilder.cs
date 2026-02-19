using Cyrena.Developer.Models;
using Cyrena.Developer.Options;
using Cyrena.Models;

namespace Cyrena.Developer.Contracts
{
    /// <summary>
    /// Use this to create project plans, different project types, add plugins
    /// </summary>
    public interface ICodeBuilder
    {
        /// <summary>
        /// Sets <see cref="Cyrena.Models.ChatConfiguration"/> project type identifier
        /// </summary>
        string Id { get; }
        /// <summary>
        /// Configures additional plugins/services and creates the <see cref="DevelopPlan"/>
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        Task<DevelopPlan> ConfigureAsync(DevelopOptions options);
        Task DeleteAsync(ChatConfiguration config);
        Task EditAsync(ChatConfiguration config, IServiceProvider services);
    }
}
