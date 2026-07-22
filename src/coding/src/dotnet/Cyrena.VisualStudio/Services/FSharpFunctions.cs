using Cyrena.Coding.Contracts;
using Cyrena.Coding.Extensions;
using Cyrena.Coding.Models;
using Cyrena.Contracts;
using Cyrena.Dotnet.Contracts;
using Cyrena.Dotnet.Models;
using Cyrena.Dotnet.Options;
using Cyrena.Extensions;
using Cyrena.Models;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace Cyrena.VisualStudio.Services
{
    internal class FSharpFunctions
    {
        private readonly ISolutionController _sln;
        private readonly IChatMessageService _chat;
        private readonly IDevelopPlanService _plan;
        public FSharpFunctions(ISolutionController sln, IChatMessageService chat, IDevelopPlanService plan)
        {
            _sln = sln;
            _chat = chat;
            _plan = plan;
        }

        [KernelFunction("set_compile_order")]
        [Description("Rebuilds the compile order of f# projects")]
        public ToolResult SetCompileOrder(string[] fsFileIds)
        {
            if (_sln.Current.ProjectTypeId != "visual.studio.fsproj")
                return new ToolResult(false, "Active project is not a F# project");
            List<DevelopFile> files = new();
            foreach (string fsFileId in fsFileIds)
            {
                if (!_plan.Plan.TryFindFile(fsFileId, out var file))
                    return new ToolResult(false, $"Unable to find {fsFileId}");
                if (!file!.Name.EndsWith(".fs"))
                    return new ToolResult(false, $"{fsFileId} is not a .fs (F#) file");
                files.Add(file);
            }
            _chat.LogInfo($"Setting compile order for {_sln.Current.ProjectName}");
            ProjectParser.SetCompileOrder(_sln.Current.ProjectFilePath, files.Select(f => f.RelativePath.TrimStart('/')));
            return new ToolResult(true, "Compile order set");
        }

        [KernelFunction("get_compile_order")]
        [Description("Gets the compile order of a f# project")]
        public ToolResult<string[]> GetCompileOrder()
        {
            if (_sln.Current.ProjectTypeId != "visual.studio.fsproj")
                return new ToolResult<string[]>(false, "Active project is not a F# project");

            var compile = ProjectParser.GetCompileOrder(_sln.Current.ProjectFilePath)
                .Select(p => {
                    var normalized = p.Replace('\\', '/');
                    return _plan.Plan.TryFindFileByPath(normalized, out var file)
                        ? $"{file!.Id}: {normalized}"
                        : normalized;
                })
                .ToArray();

            return new ToolResult<string[]>(compile);
        }
    }
}
