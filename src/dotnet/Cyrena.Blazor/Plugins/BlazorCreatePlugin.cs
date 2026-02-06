using Microsoft.SemanticKernel;
using Cyrena.Blazor.Extensions;
using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Models;
using System.ComponentModel;
using System.Numerics;

namespace Cyrena.Blazor.Plugins
{
    public class BlazorCreatePlugin
    {
        private readonly IDeveloperContext _context;
        public BlazorCreatePlugin(IDeveloperContext context)
        {
            _context = context;
        }

        [KernelFunction]
        [Description("Creates a new blazor page in the Components/Pages folder with some starter code.")]
        public ToolResult<ProjectFile> CreateBlazorPage(
            [Description("The name of the page, for example, 'Index'.")] string name,
            [Description("The url of the page that will be inserted in @page attribute.")] string? url)
        {
            name = name.Replace(".razor", "");
            var id = $"components_pages_{name}";
            if (_context.ProjectPlan.TryFindFile(id, out var file))
                return new ToolResult<ProjectFile>(file!, true, "File already exists.");
            _context.LogInfo($"Creating page {name}");
            var content = File.ReadAllText("./templates/default_blazor_page.txt");
            url = url ?? name.ToLower();
            if (url.StartsWith("/")) url = url.Substring(1);
            content = content.Replace("{url}", url).Replace("{name}", name).Replace("{root}", _context.Project.Properties["namespace"]);
            var cmp = _context.ProjectPlan.GetOrCreateFolder("components", "Components");
            var pages = _context.ProjectPlan.GetOrCreateFolder(cmp, "components_pages", "Pages");
            var nf = _context.ProjectPlan.CreateFile(pages, id, $"{name}.razor", content);
            ProjectPlan.Save(_context.ProjectPlan);
            return new ToolResult<ProjectFile>(nf);
        }

        [KernelFunction]
        [Description("Creates a new blazor component in the Components/Shared folder with some starter code.")]
        public ToolResult<ProjectFile> CreateBlazorSharedComponent(
            [Description("The name of the component, for example, 'MyForm'.")] string name)
        {
            name = name.Replace(".razor", "");
            var id = $"components_shared_{name}";
            if (_context.ProjectPlan.TryFindFile(id, out var file))
                return new ToolResult<ProjectFile>(file!, true, "File already exists.");
            _context.LogInfo($"Creating shared component {name}");
            var content = File.ReadAllText("./templates/default_blazor_shared.txt");
            content = content.Replace("{name}", name).Replace("{root}", _context.Project.Properties["namespace"]);
            var cmp = _context.ProjectPlan.GetOrCreateFolder("components", "Components");
            var shared = _context.ProjectPlan.GetOrCreateFolder(cmp, "components_shared", "Shared");
            var nf = _context.ProjectPlan.CreateFile(shared, id, $"{name}.razor", content);
            ProjectPlan.Save(_context.ProjectPlan);
            return new ToolResult<ProjectFile>(nf);
        }

        [KernelFunction]
        [Description("Creates a new blazor layout in the Components/Layout folder with some starter code.")]
        public ToolResult<ProjectFile> CreateBlazorLayout(
        [Description("The name of the layout, for example, 'MainLayout'.")] string name)
        {
            name = name.Replace(".razor", "");
            var id = $"components_layout_{name}";
            if (_context.ProjectPlan.TryFindFile(id, out var file))
                return new ToolResult<ProjectFile>(file!, true, "File already exists.");
            _context.LogInfo($"Creating layout component {name}");
            var content = File.ReadAllText("./templates/default_blazor_layout.txt");
            content = content.Replace("{name}", name).Replace("{root}", _context.Project.Properties["namespace"]);
            var cmp = _context.ProjectPlan.GetOrCreateFolder("components", "Components");
            var layouts = _context.ProjectPlan.GetOrCreateFolder(cmp, "components_layout", "Layout");
            var nf = _context.ProjectPlan.CreateFile(layouts, id, $"{name}.razor", content);
            ProjectPlan.Save(_context.ProjectPlan);
            return new ToolResult<ProjectFile>(nf);
        }

        [KernelFunction]
        [Description("Creates a new blazor component in the Components folder with some starter code.")]
        public ToolResult<ProjectFile> CreateBlazorRootComponent(
            [Description("The name of the component, for example, 'MyApp'.")] string name)
        {
            name = name.Replace(".razor", "");
            var id = $"components_{name}";
            if (_context.ProjectPlan.TryFindFile(id, out var file))
                return new ToolResult<ProjectFile>(file!, true, "File already exists.");
            _context.LogInfo($"Creating root component {name}");
            var content = File.ReadAllText("./templates/default_blazor_component.txt");
            content = content.Replace("{name}", name).Replace("{root}", _context.Project.Properties["namespace"]);
            var cmp = _context.ProjectPlan.GetOrCreateFolder("components", "Components");
            var nf = _context.ProjectPlan.CreateFile(cmp, id, $"{name}.razor", content);
            ProjectPlan.Save(_context.ProjectPlan);
            return new ToolResult<ProjectFile>(nf);
        }

        

        [KernelFunction]
        [Description("Creates a code-behind file for a component with some starter code. For example, Index.razor will get a Index.razor.cs file.")]
        public ToolResult<ProjectFile> CreateComponentCodeBehind(
            [Description("The id of the component to create code-behind for.")] string fileId)
        {
            if (!_context.ProjectPlan.TryFindFile(fileId, out var file))
                return new ToolResult<ProjectFile>(false, $"Unable to find file {fileId}");
            if (!file!.Name.EndsWith(".razor"))
                return new ToolResult<ProjectFile>(false, $"File {fileId} is not a razor component.");
            var name = file.Name.Replace(".razor", "");
            var id = $"components_layout_cs_{name}.razor";
            if (_context.ProjectPlan.TryFindFile(id, out var ext))
                return new ToolResult<ProjectFile>(ext!, true, "File already exists");
            _context.LogInfo($"Creating code-behind for {name}");
            var section = file.Id.Contains("_pages_") ? "Pages" : file.Id.Contains("_shared_") ? "Shared" : file.Id.Contains("_layout_") ? "Layout" : "";
            var content = File.ReadAllText("./templates/default_blazor_code-behind.txt");
            content = content
                .Replace("{root}", _context.Project.Properties["namespace"])
                .Replace("{name}", name)
                .Replace(".{section}", string.IsNullOrEmpty(section) ? "" : $".{section}");
            var folder = _context.ProjectPlan.GetFolderOfFile(file);
            if(folder == null)
                return new ToolResult<ProjectFile>(false, $"Unable to find correct folder.");
            var nf = _context.ProjectPlan.CreateFile(folder, id, $"{name}.razor.cs", content);
            ProjectPlan.Save(_context.ProjectPlan);
            return new ToolResult<ProjectFile>(nf);
        }
    }
}
