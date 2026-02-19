using BootstrapBlazor.Components;
using Cyrena.Contracts;
using Cyrena.Developer.Contracts;
using Cyrena.Developer.Extensions;
using Cyrena.Developer.Models;
using Cyrena.Developer.Options;
using Cyrena.Extensions;
using Cyrena.Models;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace Cyrena.Developer.Plugins
{
    internal class MVC
    {
        private readonly ISolutionController _sln;
        private readonly IChatMessageService _chat;
        private readonly IDevelopPlanService _plan;
        public MVC(ISolutionController sln, IChatMessageService chat, IDevelopPlanService plan)
        {
            _sln = sln;
            _chat = chat;
            _plan = plan;
        }

        [KernelFunction("create_controller")]
        [Description("Creates a controller in the Controllers folder of a MVC application.")]
        public ToolResult<DevelopFile> CreateController(
            [Description("The name of the controller, i.e. 'AccountController'.")]string name)
        {
            if (_sln.Current.ProjectTypeId != DotnetOptions.CsMvcApp && _sln.Current.ProjectTypeId != DotnetOptions.CsMvcLib)
                return new ToolResult<DevelopFile>(false, "Not a MVC project.");
            name = Path.GetFileNameWithoutExtension(name);
            if (!name.EndsWith("Controller"))
                name += "Controller";
            var id = $"controllers_{name}Controller";
            if (_plan.Plan.TryFindFile(id, out var file))
                return new ToolResult<DevelopFile>(false, "File already exists");
            _chat.LogInfo($"Creating controller {name}.cs");
            var content = ReadTemplate("controller.txt");
            content = content.Replace("{name}", name).Replace("{namespace}", _sln.Current["namespace"]);
            var controllers = _plan.Plan.GetOrCreateFolder("controllers", "Controllers");
            file = _plan.Plan.CreateFile(controllers, id, $"{name}.cs", content);
            return new ToolResult<DevelopFile>(file);
        }

        [KernelFunction("create_view")]
        [Description("Creates a new .cshtml view for the specified controller in the Views folder.")]
        public ToolResult<DevelopFile> CreateView(
            [Description("The file id of the controller to create the view for.")]string controllerFileId, 
            [Description("The name of the view, i.e. 'Index'.")]string name)
        {
            if (_sln.Current.ProjectTypeId != DotnetOptions.CsMvcApp && _sln.Current.ProjectTypeId != DotnetOptions.CsMvcLib)
                return new ToolResult<DevelopFile>(false, "Not a MVC project.");
            if (!_plan.Plan.TryFindFile(controllerFileId, out var controller))
                return new ToolResult<DevelopFile>(false, "Unable to find controller");
            name = Path.GetFileNameWithoutExtension(name);
            var controllerFolderName = controller!.Name.Replace("Controller", "").Replace(".cs", "");
            var views = _plan.Plan.GetOrCreateFolder("views", "Views");
            var folder = _plan.Plan.GetOrCreateFolder(views, $"views_{controllerFolderName.ToLower()}", controllerFolderName);
            var id = $"{folder.Id}_{name}";
            if(_plan.Plan.TryFindFile(id, out var _))
                return new ToolResult<DevelopFile>(false, "File already exists");
            _chat.LogInfo($"Creating view {name}.cshtml in Views/{folder.Name}");
            var content = ReadTemplate("view.txt");
            content = content.Replace("{name}", name).Replace("{namespace}", _sln.Current["namespace"]);
            var file = _plan.Plan.CreateFile(folder, id, $"{name}.cshtml", content);
            return new ToolResult<DevelopFile>(file);
        }

        [KernelFunction("create_partial")]
        [Description("Creates a new .cshtml partial view in the Views/Shared directory.")]
        public ToolResult<DevelopFile> CreateShared(
            [Description("The name of the partial view, i.e. '_Layout'.")] string name)
        {
            if (_sln.Current.ProjectTypeId != DotnetOptions.CsMvcApp && _sln.Current.ProjectTypeId != DotnetOptions.CsMvcLib)
                return new ToolResult<DevelopFile>(false, "Not a MVC project.");
            name = Path.GetFileNameWithoutExtension(name);
            var id = $"views_shared_{name}";
            if(_plan.Plan.TryFindFile(id, out var _))
                return new ToolResult<DevelopFile>(false, "File already exists");
            _chat.LogInfo($"Creating partial view {name}.cshtml in Views/Shared");
            var views = _plan.Plan.GetOrCreateFolder("views", "Views");
            var shared = _plan.Plan.GetOrCreateFolder(views, "views_shared", "Shared");
            var content = ReadTemplate("razor-partial.txt");
            content = content.Replace("{name}", name).Replace("{namespace}", _sln.Current["namespace"]);
            var file = _plan.Plan.CreateFile(shared, id, $"{name}.cshtml", content);
            return new ToolResult<DevelopFile>( file);
        }

        public string ReadTemplate(string name)
        {
            var assembly = typeof(DotnetSolution).Assembly;
            var resourceName = $"Cyrena.Developer.cs_templates.{name}";
            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
                throw new NullReferenceException($"Unable to find {resourceName}");
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
