using Cyrena.Coding.Contracts;
using Cyrena.Contracts;
using Cyrena.Dotnet.Contracts;
using Cyrena.Dotnet.Models;
using Cyrena.Dotnet.Options;

namespace Cyrena.Dotnet.Services;

public class DynamicSolutionController : ISolutionController
{
     private readonly IChatConfigurationService _config;
        private readonly IDevelopPlanService _plan;
        private readonly SolutionViewModel _sln;
        private readonly SolutionPipeline _pipe;
        public DynamicSolutionController(IChatConfigurationService config, IDevelopPlanService plan, SolutionViewModel sln)
        {
            _config = config;
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
            
        }

        public Task OverrideProjectType(string projectId, string? projectTypeId)
        {
            throw new NotImplementedException("Cannot override project type in project mode");
        }
}