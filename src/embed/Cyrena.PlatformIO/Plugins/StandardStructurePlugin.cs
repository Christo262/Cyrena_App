using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Models;
using Cyrena.PlatformIO.Contracts;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace Cyrena.PlatformIO.Plugins
{
    internal class StandardStructurePlugin
    {
        private readonly IDeveloperContext _context;
        private readonly IEnvironmentController _env;
        public StandardStructurePlugin(IDeveloperContext context, IEnvironmentController env)
        {
            _context = context;
            _env = env;
        }

        [KernelFunction]
        [Description("Gets information about the current PlatformIO environment you are working on.")]
        public Dictionary<string, string?> GetPlatformIOEnvironment()
        {
            if (_env.Current == null)
                return new Dictionary<string, string?>() { { "ERROR", "No active environments" } };
            var model = new Dictionary<string, string?>();
            model["environment"] = _env.Current.Name;
            foreach (var item in _env.Current.Properties)
                model[item.Id] = item.Value;
            return model;
        }

        [KernelFunction]
        [Description("Creates a new header file (*.h) in the project's include folder.")]
        public ToolResult<ProjectFile> Create_Include_Header(
            [Description("Name of the file, example 'my_class'.")]string name)
        {
            return CreateFile("include", "h", name);
        }

        [KernelFunction]
        [Description("Creates a new c file (*.c) in the project's src folder.")]
        public ToolResult<ProjectFile> Create_Src_C_File(
            [Description("Name of the file, example 'my_class'.")] string name)
        {
            return CreateFile("src", "c", name);
        }

        [KernelFunction]
        [Description("Creates a new C++ file (*.cpp) in the project's src folder.")]
        public ToolResult<ProjectFile> Create_Src_Cpp_File(
            [Description("Name of the file, example 'my_class'.")] string name)
        {
            return CreateFile("src", "cpp", name);
        }

        private ToolResult<ProjectFile> CreateFile(string dir, string ext, string name)
        {
            name = Path.GetFileNameWithoutExtension(name);
            var id = $"{dir}_{ext}_{name}";
            if (_context.ProjectPlan.TryFindFile(id, out var file))
                return new ToolResult<ProjectFile>(file!, true, "File already exists");
            _context.LogInfo($"Creating file {dir}/{name}.{ext}");
            var target = _context.ProjectPlan.GetOrCreateFolder(dir, dir);
            var nf = _context.ProjectPlan.CreateFile(target, id, $"{name}.{ext}", null);
            ProjectPlan.Save(_context.ProjectPlan);
            return new ToolResult<ProjectFile>(nf);
        }
    }
}
