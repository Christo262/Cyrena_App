using Cyrena.Models;
using Microsoft.SemanticKernel;

namespace Cyrena.Contracts
{
    /// <summary>
    /// Adds additional services/features/functions to a <see cref="IAssistantMode"/>
    /// </summary>
    public interface IAssistantPlugin
    {
        /// <summary>
        /// Id of the plugin
        /// </summary>
        string Id { get; }
        /// <summary>
        /// The <see cref="IAssistantMode.Id"/> for the modes this is applicable to. Leave empty for all.
        /// </summary>
        string[] Modes { get; }
        /// <summary>
        /// Priority of execution
        /// </summary>
        int Priority { get; }
        /// <summary>
        /// If this plugin can be deactivated, set to false
        /// </summary>
        bool Required { get; }
        /// <summary>
        /// Display title of the plugin
        /// </summary>
        string Title { get; }
        /// <summary>
        /// Applies the plugin
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        Task LoadAsync(CyrenaKernelBuilder builder);
    }
}
