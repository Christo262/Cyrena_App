using Cyrena.Models;

namespace Cyrena.Contracts
{
    /// <summary>
    /// Configures a new project with structure, kernel plugins and context prompt
    /// </summary>
    public interface IProjectConfigurator
    {
        /// <summary>
        /// Type of project this configures, ex: blazor-app
        /// </summary>
        string ProjectType { get; }
        /// <summary>
        /// Display name for UI
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Display description for UI
        /// </summary>
        string? Description { get; }
        /// <summary>
        /// Bootstrap Icon for UI
        /// </summary>
        string? Icon { get; }

        /// <summary>
        /// Creates the project plan, checks all the files, adds structure kernel plugins, feeds prompt. Throws exception of error occurs that cant be recovered from, i.e. RootDirectory is gone.
        /// </summary>
        /// <param name="builder"><see cref="IDeveloperContextBuilder"/></param>
        /// <returns><see cref="ProjectPlan"/></returns>
        Task<ProjectPlan> InitializeAsync(IDeveloperContextBuilder builder);
        /// <summary>
        /// Creates a new project of this type,
        /// </summary>
        /// <returns>true on success or false. This informs UI about changes</returns>
        Task<bool> CreateNewAsync();
        /// <summary>
        /// Edit a project
        /// </summary>
        /// <param name="project"><see cref="Project"/></param>
        /// <returns></returns>
        Task EditAsync(Project project);
    }
}
