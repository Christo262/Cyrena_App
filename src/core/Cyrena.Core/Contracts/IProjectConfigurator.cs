using Cyrena.Models;

namespace Cyrena.Contracts
{
    /// <summary>
    /// What type of projects are supported
    /// </summary>
    public interface IProjectConfigurator
    {
        /// <summary>
        /// Type of project this configures, ex: blazor-server
        /// </summary>
        string ProjectType { get; }
        string Name { get; }
        string? Description { get; }
        string? Icon { get; }

        Task<ProjectPlan> InitializeAsync(IDeveloperContextBuilder builder);

        Task<bool> CreateNewAsync();
        Task EditAsync(Project project);
    }
}
