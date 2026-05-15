using Cyrena.Dotnet.Models;
using Cyrena.Models;

namespace Cyrena.Dotnet.Services
{
    internal class SolutionPipeline : EventPipeline
    {
        public IDisposable WatchProjectChange(Action<ProjectModel> callback) => this.ConfigurePipe("proj_change", callback);
        public void InvokeProjectChange(ProjectModel proj) => this.InvokePipeline("proj_change", proj);
    }
}
