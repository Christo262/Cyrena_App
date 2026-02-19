using Cyrena.Developer.Models;

namespace Cyrena.Developer.Contracts
{
    /// <summary>
    /// Service to switch between active projects. Kernel locked
    /// </summary>
    public interface ISolutionController : IDisposable
    {
        Task SetTargetProject(ProjectViewModel current);
        IEnumerable<ProjectViewModel> GetValidProjects();
        ProjectViewModel Current { get; }
        IDisposable OnProjectChange(Action<ProjectViewModel> cb);
    }
}
