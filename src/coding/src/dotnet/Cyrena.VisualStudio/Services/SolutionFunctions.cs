using System.ComponentModel;
using Cyrena.Contracts;
using Cyrena.Dotnet.Contracts;
using Cyrena.Dotnet.Models;
using Cyrena.Extensions;
using Cyrena.Models;
using Microsoft.SemanticKernel;

namespace Cyrena.VisualStudio.Services;

public class SolutionFunctions
{
    private readonly ISolutionController _sln;
    private readonly IChatMessageService _chat;
    public SolutionFunctions(ISolutionController sln, IChatMessageService chat)
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