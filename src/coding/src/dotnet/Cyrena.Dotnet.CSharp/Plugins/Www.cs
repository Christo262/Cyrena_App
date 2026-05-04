using Cyrena.Coding.Contracts;
using Cyrena.Coding.Extensions;
using Cyrena.Coding.Models;
using Cyrena.Contracts;
using Cyrena.Dotnet.Contracts;
using Cyrena.Dotnet.CSharp.Services;
using Cyrena.Extensions;
using Cyrena.Models;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace Cyrena.Dotnet.CSharp.Plugins
{
    internal class Www
    {
        private readonly ISolutionController _sln;
        private readonly IChatMessageService _chat;
        private readonly IDevelopPlanService _plan;
        public Www(ISolutionController sln, IChatMessageService chat, IDevelopPlanService plan)
        {
            _sln = sln;
            _chat = chat;
            _plan = plan;
        }

        [KernelFunction("stylesheet")]
        [Description("Creates a new css file in the wwwroot/css folder with some starter code.")]
        public ToolResult<DevelopFile> CreateStylesheet(
            [Description("The name of the css file, for example, 'my-styles'.")] string name)
        {
            try
            {
                if (_sln.Current.ProjectTypeId != BlazorClassLibrary.Id && _sln.Current.ProjectTypeId != BlazorApplication.Id &&
                _sln.Current.ProjectTypeId != MvcApplication.Id && _sln.Current.ProjectTypeId != MvcLibrary.Id)
                    throw new Exception($"Attempted creating {name}.css for {_sln.Current.ProjectTypeId} which is not a web application");
                name = Path.GetFileNameWithoutExtension(name);
                var id = $"styles_{name}";
                if (_plan.Plan.TryFindFile(id, out var file))
                    return new ToolResult<DevelopFile>(file!, true, "File already exists.");
                _chat.LogInfo($"Creating stylesheet {name}");
                var www = _plan.Plan.GetOrCreateFolder("wwwroot", "wwwroot");
                var style = _plan.Plan.GetOrCreateFolder(www, "wwwroot_css", "css");
                var model = _plan.Plan.CreateFile(style, id, $"{name}.css", $"body {{ {Environment.NewLine} }}");
                _plan.InvokeFileCreated(model);
                return new ToolResult<DevelopFile>(model);
            }catch(Exception ex)
            {
                _chat.LogError(ex.Message);
                return new ToolResult<DevelopFile>(false, ex.Message);
            }
        }

        [KernelFunction("javascript")]
        [Description("Creates a new javascript file in the wwwroot/js folder with some starter code.")]
        public ToolResult<DevelopFile> CreateJavaScript(
            [Description("The name of the javascript file, for example, 'my-scripts'.")] string name)
        {
            try
            {
                if (_sln.Current.ProjectTypeId != BlazorClassLibrary.Id && _sln.Current.ProjectTypeId != BlazorApplication.Id &&
                _sln.Current.ProjectTypeId != MvcApplication.Id && _sln.Current.ProjectTypeId != MvcLibrary.Id)
                    throw new Exception($"Attempted creating {name}.js for {_sln.Current.ProjectTypeId} which is not a web application");
                name = Path.GetFileNameWithoutExtension(name);
                var id = $"script_{name}";
                if (_plan.Plan.TryFindFile(id, out var file))
                    return new ToolResult<DevelopFile>(file!, true, "File already exists.");
                _chat.LogInfo($"Creating javascript {name}");
                var www = _plan.Plan.GetOrCreateFolder("wwwroot", "wwwroot");
                var scripts = _plan.Plan.GetOrCreateFolder(www, "scripts", "js");
                var model = _plan.Plan.CreateFile(scripts, id, $"{name}.js", $"function foo() {{ {Environment.NewLine} }}");
                _plan.InvokeFileCreated(model);
                return new ToolResult<DevelopFile>(model);
            }catch (Exception ex)
            {
                _chat.LogError(ex.Message);
                return new ToolResult<DevelopFile>(false, ex.Message);
            }
        }
    }
}
