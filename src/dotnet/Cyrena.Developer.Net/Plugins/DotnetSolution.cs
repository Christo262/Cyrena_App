using Cyrena.Contracts;
using Cyrena.Developer.Contracts;
using Cyrena.Developer.Models;
using Cyrena.Extensions;
using Cyrena.Models;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace Cyrena.Developer.Plugins
{
    public class DotnetSolution
    {
        private readonly ISolutionController _sln;
        private readonly IChatMessageService _chat;
        public DotnetSolution(ISolutionController sln, IChatMessageService chat)
        {
            _sln = sln;
            _chat = chat;
        }

        [KernelFunction("get_projects")]
        [Description("Lists all projects in the current solution.")]
        public IEnumerable<ProjectViewModel> GetProjects()
        {
            return _sln.GetValidProjects().Select(x => new ProjectViewModel(x));
        }

        [KernelFunction("set_target_project")]
        [Description("Sets a different project as the *target* project, allowing development on that project.")]
        public async Task<ToolResult<ProjectModel>> SetCurrentProject(
            [Description("The id of the project to target.")]string projectId)
        {
            var projs = _sln.GetValidProjects();
            var proj = projs.FirstOrDefault(x => x.Id == projectId);
            if (proj == null)
                return new ToolResult<ProjectModel>(false, $"No valid project found with id {projectId}");
            await _chat.LogInfo($"Changing target project: {proj.ProjectName}, {proj.ProjectTypeId}");
            await _sln.SetTargetProject(proj);
            return new ToolResult<ProjectModel>(_sln.Current, true, "Target project changed.");
        }

        [KernelFunction("get_target_project")]
        [Description("Gets the current *target* project that can be developed.")]
        public ProjectModel GetTargetProjectAndPlan()
        {
            return _sln.Current;
        }
    }
}
