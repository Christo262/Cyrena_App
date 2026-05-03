using Cyrena.Coding.Contracts;
using Cyrena.Coding.Models;
using Cyrena.Coding.Options;
using Cyrena.Contracts;
using Cyrena.Dotnet.Contracts;
using Cyrena.Dotnet.Models;
using Cyrena.Dotnet.Options;

namespace Cyrena.Dotnet.CSharp.Services
{
    internal class DevelopPlanIndexer : IDevelopPlanIndexer
    {
        private readonly ISolutionController _sln;
        private readonly IEnumerable<IDotnetProjectType> _project_types;
        private readonly IChatConfigurationService _config;
        public DevelopPlanIndexer(ISolutionController sln, IEnumerable<IDotnetProjectType> project_types, IChatConfigurationService config)
        {
            _sln = sln;
            _project_types = project_types;
            _config = config;
        }

        public DevelopPlan? RefreshPlan(DevelopPlan current)
        {
            var sln_path = _config.Config[DotnetOptions.SolutionFilePath];
            if (!string.IsNullOrEmpty(sln_path))
                return RefreshSolution(current, sln_path);
            var proj = _config.Config[DotnetOptions.ProjectFilePath];
            if(!string.IsNullOrEmpty(proj))
                return RefreshProject(current, proj);
            return null;
        }

        private DevelopPlan? RefreshSolution(DevelopPlan current, string absolute_sln_path)
        {
            if(string.IsNullOrEmpty(absolute_sln_path) || !File.Exists(absolute_sln_path))
                return null;
            var info = SolutionParser.GetProjectDetails(absolute_sln_path);
            var sln_dir = Path.GetDirectoryName(absolute_sln_path);
            var current_id = _sln.Current.Id;
            _sln.Sln.Projects.Clear();
            foreach (var item in info)
            {
                var project_type = _project_types.FirstOrDefault(x => x.IsOfType(item));
                var fi = new FileInfo(item.AbsolutePath);
                var id = $"{fi.DirectoryName?.Replace(sln_dir ?? "", "").Replace("\\", "_")}_{item.ProjectName}";
                ProjectModel project = new ProjectModel()
                {
                    ConversationId = _config.Config.Id,
                    ProjectFilePath = item.AbsolutePath,
                    ProjectName = item.ProjectName,
                    ProjectDirectory = fi.DirectoryName!,
                    ProjectTypeId = project_type?.Id,
                    ProjectTypeName = project_type?.ProjectTypeName,
                    Id = $"{fi.DirectoryName?.Replace(sln_dir ?? "", "").Replace("\\", "_")}_{item.ProjectName}"
                };
                _sln.Sln.Projects.Add(project);

                if (project_type != null)
                {
                    project.ProjectTypeId = project_type.Id;
                    project.ProjectTypeName = project_type.ProjectTypeName;
                    project_type.IndexPlan(project);
                }
            }
            var new_current = _sln.GetValidProjects().FirstOrDefault(x => x.Id == current_id);
            if(new_current != null)
            {
                _sln.SetTargetProject(new_current);
                return new_current.Plan;
            }
            return null;
        }

        private DevelopPlan? RefreshProject(DevelopPlan current, string absolute_proj_path)
        {
            if (string.IsNullOrEmpty(absolute_proj_path) || !File.Exists(absolute_proj_path))
                return null;

            var csproj = ProjectParser.ParseProject(absolute_proj_path);
            _config.Config[DotnetOptions.Namespace] = csproj.RootNamespace;
            _config.Config[DevelopOptions.RootDirectory] = Path.GetDirectoryName(absolute_proj_path);
            var id = _sln.Current.Id;
            var typeId = _sln.Current.ProjectTypeId;
            var name = _sln.Current.ProjectTypeName;
            _sln.Sln.Projects.Clear();
            var project = new ProjectModel()
            {
                Id = id,
                ConversationId = _config.Config.Id,
                ProjectFilePath = absolute_proj_path,
                ProjectName = Path.GetFileName(absolute_proj_path),
                ProjectDirectory = _config.Config[DevelopOptions.RootDirectory]!,
                ProjectTypeId = typeId,
                ProjectTypeName = name
            };
            _sln.Sln.Projects.Add(project);
            var idxer = _project_types.FirstOrDefault(x => x.Id == project.ProjectTypeId);
            if(idxer == null)
                return null;
            var plan = idxer.IndexPlan(project);
            _sln.SetTargetProject(project);
            return plan;
        }
    }
}
