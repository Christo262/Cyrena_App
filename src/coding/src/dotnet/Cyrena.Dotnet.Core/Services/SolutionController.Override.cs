using Cyrena.Coding.Contracts;
using Cyrena.Contracts;
using Cyrena.Dotnet.Contracts;
using Cyrena.Dotnet.Models;
using Cyrena.Dotnet.Options;
using Cyrena.Extensions;
using Cyrena.Models;
using Cyrena.Persistence.Contracts;

namespace Cyrena.Dotnet.Services
{
    internal class SolutionControllerOverride : ISolutionController
    {
        private readonly IStore<ProjectTypeOverride> _store;
        private readonly IChatConfigurationService _config;
        private readonly IEnumerable<IDotnetProjectType> _project_types;
        private readonly IDevelopPlanService _plan;
        private readonly SolutionViewModel _sln;
        private readonly SolutionPipeline _pipe;
        public SolutionControllerOverride(IStore<ProjectTypeOverride> store, IChatConfigurationService config, IEnumerable<IDotnetProjectType> project_types, IDevelopPlanService plan, SolutionViewModel sln)
        {
            _store = store;
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
            {
                var proj_type = _project_types.FirstOrDefault(x => x.Id == current.ProjectTypeId);
                if (proj_type == null)
                    throw new InvalidOperationException($"{current.Id} is unknown project type");
                proj_type.IndexPlan(current);
            }
            _current = current;
            _plan.SetPlan(current.Plan!);
            _config.Config[DotnetOptions.LastProject] = current.Id;
            _pipe.InvokeProjectChange(_current);
            await _config.SaveConfigurationAsync();
        }

        public IEnumerable<ProjectModel> GetValidProjects()
        {
            return _sln.Projects.Where(x => x.Plan != null);
        }

        public IEnumerable<ProjectModel> GetAllProjects()
        {
            return _sln.Projects;
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

        public async Task OverrideProjectType(string projectId, string? projectTypeId)
        {
            var project = _sln.Projects.FirstOrDefault(x => x.Id == projectId);
            if(project == null)
                throw new NullReferenceException($"Unable to find project {projectId}");
            var project_type = _project_types.FirstOrDefault(x => x.Id == projectTypeId);
            if (!string.IsNullOrEmpty(projectTypeId) && project_type == null)
                throw new NullReferenceException($"Unable to find project type indexer {projectTypeId}");

            var ext = await _store.FindAsync(x => x.Id == projectId);
            if(ext == null)
                ext = new ProjectTypeOverride(projectId);
            ext.ProjectTypeId = projectTypeId;
            await _store.SaveAsync(ext);
            if (project_type != null)
                project_type.IndexPlan(project);
            else
                project.Plan = null;
        }

        internal async Task ApplyOverrides(CancellationToken ct = default!)
        {
            var ovrs = await _store.FindManyAsync(x => true, ct:ct);
            foreach(var item in _sln.Projects)
            {
                var ovr = ovrs.FirstOrDefault(x => x.Id == item.Id);
                if(ovr != null)
                    item.ProjectTypeId = ovr.ProjectTypeId;
            }
        }
    }

    internal class SolutionOverrideStartupTask : IStartupTask
    {
        private readonly ISolutionController _sln;
        public SolutionOverrideStartupTask(ISolutionController sln)
        {
            _sln = sln;
        }

        public int Order => 10;

        public async Task RunAsync(CancellationToken cancellationToken = default)
        {
            if (_sln is SolutionControllerOverride ovr)
                await ovr.ApplyOverrides(cancellationToken);
        }
    }
}
