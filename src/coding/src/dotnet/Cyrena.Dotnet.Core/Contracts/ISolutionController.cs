using Cyrena.Dotnet.Models;

namespace Cyrena.Dotnet.Contracts
{
    /// <summary>
    /// Service to switch between active projects. Kernel locked
    /// </summary>
    public interface ISolutionController : IDisposable
    {
        Task SetTargetProject(ProjectModel current);
        IEnumerable<ProjectModel> GetValidProjects();
        void RefreshProjectPlans();
        ProjectModel Current { get; }
        SolutionViewModel Sln { get; }
        IDisposable OnProjectChange(Action<ProjectModel> cb);
    }
}
