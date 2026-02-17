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
    internal class Blazor
    {
        private readonly ISolutionController _sln;
        private readonly IChatMessageService _chat;
        private readonly IDevelopPlanService _plan;
        public Blazor(ISolutionController sln, IChatMessageService chat, IDevelopPlanService plan)
        {
            _sln = sln;
            _chat = chat;
            _plan = plan;
        }

        [KernelFunction("create_page")]
        [Description("Creates a new blazor page in the Components/Pages folder with some starter code.")]
        public ToolResult<DevelopFile> CreateBlazorPage(
            [Description("The name of the page, for example, 'Index'.")] string name,
            [Description("The url of the page that will be inserted in @page attribute.")] string? url)
        {
            if(_sln.Current.ProjectTypeId != DotnetOptions.CsBlazorLibrary && _sln.Current.ProjectTypeId != DotnetOptions.CsBlazorApp)
                return new ToolResult<DevelopFile>(false, "Not a blazor project.");
            name = Path.GetFileNameWithoutExtension(name);
            var id = $"components_pages_{name}";
            if (_plan.Plan.TryFindFile(id, out var file))
                return new ToolResult<DevelopFile>(file!, true, "File already exists.");
            _chat.LogInfo($"Creating page {name}");
            var content = ReadTemplate("blazor-page.txt");
            url = url ?? name.ToLower();
            if (url.StartsWith("/")) url = url.Substring(1);
            content = content.Replace("{url}", url).Replace("{name}", name).Replace("{namespace}", _sln.Current["namespace"]);
            var cmp = _plan.Plan.GetOrCreateFolder("components", "Components");
            var pages = _plan.Plan.GetOrCreateFolder(cmp, "components_pages", "Pages");
            var nf = _plan.Plan.CreateFile(pages, id, $"{name}.razor", content);
            return new ToolResult<DevelopFile>(nf);
        }

        [KernelFunction("create_shared")]
        [Description("Creates a new blazor component in the Components/Shared folder with some starter code.")]
        public ToolResult<DevelopFile> CreateBlazorSharedComponent(
            [Description("The name of the component, for example, 'MyForm'.")] string name)
        {
            if (_sln.Current.ProjectTypeId != DotnetOptions.CsBlazorLibrary && _sln.Current.ProjectTypeId != DotnetOptions.CsBlazorApp)
                return new ToolResult<DevelopFile>(false, "Not a blazor project.");
            name = Path.GetFileNameWithoutExtension(name);
            var id = $"components_shared_{name}";
            if (_plan.Plan.TryFindFile(id, out var file))
                return new ToolResult<DevelopFile>(file!, true, "File already exists.");
            _chat.LogInfo($"Creating shared component {name}");
            var content = ReadTemplate("blazor-shared.txt");
            content = content.Replace("{name}", name).Replace("{namespace}", _sln.Current["namespace"]);
            var cmp = _plan.Plan.GetOrCreateFolder("components", "Components");
            var shared = _plan.Plan.GetOrCreateFolder(cmp, "components_shared", "Shared");
            var nf = _plan.Plan.CreateFile(shared, id, $"{name}.razor", content);
            return new ToolResult<DevelopFile>(nf);
        }

        [KernelFunction("create_layout")]
        [Description("Creates a new blazor layout in the Components/Layout folder with some starter code.")]
        public ToolResult<DevelopFile> CreateBlazorLayout(
        [Description("The name of the layout, for example, 'MainLayout'.")] string name)
        {
            if (_sln.Current.ProjectTypeId != DotnetOptions.CsBlazorLibrary && _sln.Current.ProjectTypeId != DotnetOptions.CsBlazorApp)
                return new ToolResult<DevelopFile>(false, "Not a blazor project.");
            name = Path.GetFileNameWithoutExtension(name);
            var id = $"components_layout_{name}";
            if (_plan.Plan.TryFindFile(id, out var file))
                return new ToolResult<DevelopFile>(file!, true, "File already exists.");
            _chat.LogInfo($"Creating layout component {name}");
            var content = ReadTemplate("blazor-layout.txt");
            content = content.Replace("{name}", name).Replace("{namespace}", _sln.Current["namespace"]);
            var cmp = _plan.Plan.GetOrCreateFolder("components", "Components");
            var layouts = _plan.Plan.GetOrCreateFolder(cmp, "components_layout", "Layout");
            var nf = _plan.Plan.CreateFile(layouts, id, $"{name}.razor", content);
            return new ToolResult<DevelopFile>(nf);
        }

        [KernelFunction("create_root")]
        [Description("Creates a new blazor component in the Components folder with some starter code.")]
        public ToolResult<DevelopFile> CreateBlazorRootComponent(
            [Description("The name of the component, for example, 'MyApp'.")] string name)
        {
            if (_sln.Current.ProjectTypeId != DotnetOptions.CsBlazorLibrary && _sln.Current.ProjectTypeId != DotnetOptions.CsBlazorApp)
                return new ToolResult<DevelopFile>(false, "Not a blazor project.");
            name = Path.GetFileNameWithoutExtension(name);
            var id = $"components_{name}";
            if (_plan.Plan.TryFindFile(id, out var file))
                return new ToolResult<DevelopFile>(file!, true, "File already exists.");
            _chat.LogInfo($"Creating root component {name}");
            var content = ReadTemplate("blazor-root.txt");
            content = content.Replace("{name}", name).Replace("{namespace}", _sln.Current["namespace"]);
            var cmp = _plan.Plan.GetOrCreateFolder("components", "Components");
            var nf = _plan.Plan.CreateFile(cmp, id, $"{name}.razor", content);
            return new ToolResult<DevelopFile>(nf);
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
