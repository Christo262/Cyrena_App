using Cyrena.Contracts;
using Cyrena.Developer.Contracts;
using Cyrena.Developer.Extensions;
using Cyrena.Developer.Models;
using Cyrena.Developer.Options;
using Cyrena.Extensions;
using Cyrena.Models;
using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using System.Text;

namespace Cyrena.Developer.Plugins
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
            if (_sln.Current.ProjectTypeId != DotnetOptions.CsBlazorLibrary || _sln.Current.ProjectTypeId != DotnetOptions.CsBlazorApp)
                return new ToolResult<DevelopFile>(false, "Not a web project.");
            name = Path.GetFileNameWithoutExtension(name);
            var id = $"styles_{name}";
            if (_plan.Plan.TryFindFile(id, out var file))
                return new ToolResult<DevelopFile>(file!, true, "File already exists.");
            _chat.LogInfo($"Creating stylesheet {name}");
            var www = _plan.Plan.GetOrCreateFolder("wwwroot", "wwwroot");
            var style = _plan.Plan.GetOrCreateFolder(www, "wwwroot_css", "css");
            var model = _plan.Plan.CreateFile(style, id, $"{name}.css", $"body {{ {Environment.NewLine} }}");
            return new ToolResult<DevelopFile>(model);
        }

        [KernelFunction("javascript")]
        [Description("Creates a new javascript file in the wwwroot/js folder with some starter code.")]
        public ToolResult<DevelopFile> CreateJavaScript(
            [Description("The name of the javascript file, for example, 'my-scripts'.")] string name)
        {
            if (_sln.Current.ProjectTypeId != DotnetOptions.CsBlazorLibrary || _sln.Current.ProjectTypeId != DotnetOptions.CsBlazorApp)
                return new ToolResult<DevelopFile>(false, "Not a web project.");
            name = Path.GetFileNameWithoutExtension(name);
            var id = $"script_{name}";
            if (_plan.Plan.TryFindFile(id, out var file))
                return new ToolResult<DevelopFile>(file!, true, "File already exists.");
            _chat.LogInfo($"Creating javascript {name}");
            var www = _plan.Plan.GetOrCreateFolder("wwwroot", "wwwroot");
            var scripts = _plan.Plan.GetOrCreateFolder(www, "scripts", "js");
            var model = _plan.Plan.CreateFile(scripts, id, $"{name}.js", $"function foo() {{ {Environment.NewLine} }}");
            return new ToolResult<DevelopFile>(model);
        }
    }
}
