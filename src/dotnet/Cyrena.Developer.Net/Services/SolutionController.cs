using Cyrena.Contracts;
using Cyrena.Developer.Contracts;
using Cyrena.Developer.Models;
using Cyrena.Developer.Options;
using Cyrena.Models;

namespace Cyrena.Developer.Services
{
    internal class SolutionController : ISolutionController
    {
        private readonly IChatConfigurationService _config;
        private readonly IEnumerable<IDotnetProjectType> _project_types;
        private readonly IDevelopPlanService _plan;
        private readonly SolutionViewModel _sln;
        private readonly SolutionPipeline _pipe;
        public SolutionController(IChatConfigurationService config, IEnumerable<IDotnetProjectType> project_types, IDevelopPlanService plan, SolutionViewModel sln)
        {
            _config = config;
            _project_types = project_types;
            _plan = plan;
            _sln = sln;
            _current = _sln.Projects.First(x => x.Id == _config.Config[DotnetOptions.LastProject]);
            _pipe = new SolutionPipeline();
        }

        private ProjectViewModel _current { get; set; } = default!;

        public async Task SetTargetProject(ProjectViewModel current)
        {
            if (current.Plan == null)
                return;
            _current = current;
            _plan.SetPlan(current.Plan);
            _config.Config[DotnetOptions.LastProject] = current.Id;
            _pipe.InvokeProjectChange(_current);    
            await _config.SaveConfigurationAsync();
        }

        public IEnumerable<ProjectViewModel> GetValidProjects()
        {
            return _sln.Projects.Where(x => x.Plan != null);
        }

        public IDisposable OnProjectChange(Action<ProjectViewModel> cb) => _pipe.WatchProjectChange(cb);

        public ProjectViewModel Current => _current;

        public void Dispose()
        {
            _pipe.Dispose();
        }

        internal class SolutionPipeline : EventPipeline
        {
            public IDisposable WatchProjectChange(Action<ProjectViewModel> callback) => this.ConfigurePipe("proj_change", callback);
            public void InvokeProjectChange(ProjectViewModel proj) => this.InvokePipeline("proj_change", proj);
        }
    }
}
