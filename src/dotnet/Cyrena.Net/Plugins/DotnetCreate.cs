using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Models;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace Cyrena.Net.Plugins
{
    public class DotnetCreate
    {
        private readonly IDeveloperContext _context;
        public DotnetCreate(IDeveloperContext context)
        {
            _context = context;
        }

        [KernelFunction("interface")]
        [Description("Creates a new interface in the Contracts folder with some starter code.")]
        public ToolResult<ProjectFile> CreateInterface(
            [Description("The name of the interface, for example, 'ISomeService'.")] string name)
        {
            name = name.Replace(".cs", "");
            var id = $"contracts_{name}";
            if (_context.ProjectPlan.TryFindFile(id, out var file))
                return new ToolResult<ProjectFile>(file!, true, "File already exists.");
            _context.LogInfo($"Creating interface {name}");
            var content = File.ReadAllText("./templates/default_interface.txt");
            content = content.Replace("{name}", name).Replace("{root}", _context.Project.Properties["namespace"]);
            var contracts = _context.ProjectPlan.GetOrCreateFolder("contracts", "Contracts");
            var nf = _context.ProjectPlan.CreateFile(contracts, id, $"{name}.cs", content);
            ProjectPlan.Save(_context.ProjectPlan);
            return new ToolResult<ProjectFile>(nf);
        }

        [KernelFunction("service")]
        [Description("Creates a new class in the Services folder with some starter code.")]
        public ToolResult<ProjectFile> CreateService(
            [Description("The name of the service, for example, 'SomeService'.")] string name)
        {
            name = name.Replace(".cs", "");
            var id = $"services_{name}";
            if (_context.ProjectPlan.TryFindFile(id, out var file))
                return new ToolResult<ProjectFile>(file!, true, "File already exists.");
            _context.LogInfo($"Creating service {name}");
            var content = File.ReadAllText("./templates/default_service.txt");
            content = content.Replace("{name}", name).Replace("{root}", _context.Project.Properties["namespace"]);
            var services = _context.ProjectPlan.GetOrCreateFolder("services", "Services");
            var nf = _context.ProjectPlan.CreateFile(services, id, $"{name}.cs", content);
            ProjectPlan.Save(_context.ProjectPlan);
            return new ToolResult<ProjectFile>(nf);
        }

        [KernelFunction("model")]
        [Description("Creates a new class in the Models folder with some starter code.")]
        public ToolResult<ProjectFile> CreateModel(
            [Description("The name of the model, for example, 'MyModel'.")] string name)
        {
            name = name.Replace(".cs", "");
            var id = $"models_{name}";
            if (_context.ProjectPlan.TryFindFile(id, out var file))
                return new ToolResult<ProjectFile>(file!, true, "File already exists.");
            _context.LogInfo($"Creating model {name}");
            var content = File.ReadAllText("./templates/default_model.txt");
            content = content.Replace("{name}", name).Replace("{root}", _context.Project.Properties["namespace"]);
            var models = _context.ProjectPlan.GetOrCreateFolder("models", "Models");
            var nf = _context.ProjectPlan.CreateFile(models, id, $"{name}.cs", content);
            ProjectPlan.Save(_context.ProjectPlan);
            return new ToolResult<ProjectFile>(nf);
        }

        [KernelFunction("option")]
        [Description("Creates a new class in the Options folder with some starter code.")]
        public ToolResult<ProjectFile> CreateOption(
    [Description("The name of the option, for example, 'MyOptions'.")] string name)
        {
            name = name.Replace(".cs", "");
            var id = $"options_{name}";
            if (_context.ProjectPlan.TryFindFile(id, out var file))
                return new ToolResult<ProjectFile>(file!, true, "File already exists.");
            _context.LogInfo($"Creating options {name}");
            var content = File.ReadAllText("./templates/default_option.txt");
            content = content.Replace("{name}", name).Replace("{root}", _context.Project.Properties["namespace"]);
            var models = _context.ProjectPlan.GetOrCreateFolder("options", "Options");
            var nf = _context.ProjectPlan.CreateFile(models, id, $"{name}.cs", content);
            ProjectPlan.Save(_context.ProjectPlan);
            return new ToolResult<ProjectFile>(nf);
        }


        [KernelFunction("extension")]
        [Description("Creates a new class in the Extensions folder with some starter code.")]
        public ToolResult<ProjectFile> CreateExtension(
            [Description("The name of the extension, for example, 'MyModelExtensions'.")] string name)
        {
            name = name.Replace(".cs", "");
            var id = $"extensions_{name}";
            if (_context.ProjectPlan.TryFindFile(id, out var file))
                return new ToolResult<ProjectFile>(file!, true, "File already exists.");
            _context.LogInfo($"Creating extension {name}");
            var content = File.ReadAllText("./templates/default_extension.txt");
            content = content.Replace("{name}", name).Replace("{root}", _context.Project.Properties["namespace"]);
            var models = _context.ProjectPlan.GetOrCreateFolder("extensions", "Extensions");
            var nf = _context.ProjectPlan.CreateFile(models, id, $"{name}.cs", content);
            ProjectPlan.Save(_context.ProjectPlan);
            return new ToolResult<ProjectFile>(nf);
        }
    }
}
