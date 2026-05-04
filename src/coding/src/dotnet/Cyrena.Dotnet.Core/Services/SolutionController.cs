using Cyrena.Coding.Contracts;
using Cyrena.Contracts;
using Cyrena.Dotnet.Contracts;
using Cyrena.Dotnet.Models;
using Cyrena.Dotnet.Options;
using Cyrena.Models;

namespace Cyrena.Dotnet.Services
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
            _current = _sln.Projects.First(x => x.Id == _config.Config[DotnetOptions.LastProject])!;
            _pipe = new SolutionPipeline();
        }

        private ProjectModel _current { get; set; } = default!;

        public async Task SetTargetProject(ProjectModel current)
        {
            if (current.Plan == null)
                return;
            _current = current;
            _plan.SetPlan(current.Plan);
            _config.Config[DotnetOptions.LastProject] = current.Id;
            _pipe.InvokeProjectChange(_current);    
            await _config.SaveConfigurationAsync();
        }

        public IEnumerable<ProjectModel> GetValidProjects()
        {
            return _sln.Projects.Where(x => x.Plan != null);
        }

        public IDisposable OnProjectChange(Action<ProjectModel> cb) => _pipe.WatchProjectChange(cb);

        public ProjectModel Current => _current;
        public SolutionViewModel Sln => _sln;

        public void Dispose()
        {
            _pipe.Dispose();
        }

        public void RefreshProjectPlans()
        {
            foreach (var project in _sln.Projects)
            {
                var type = _project_types.FirstOrDefault(x => x.Id == project.ProjectTypeId);
                if (type != null)
                    type.IndexPlan(project);
            }
        }

        internal class SolutionPipeline : EventPipeline
        {
            public IDisposable WatchProjectChange(Action<ProjectModel> callback) => this.ConfigurePipe("proj_change", callback);
            public void InvokeProjectChange(ProjectModel proj) => this.InvokePipeline("proj_change", proj);
        }
    }
}
