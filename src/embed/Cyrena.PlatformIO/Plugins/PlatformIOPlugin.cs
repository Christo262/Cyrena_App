using Cyrena.Contracts;
using Cyrena.Developer.Contracts;
using Cyrena.Developer.Extensions;
using Cyrena.Developer.Models;
using Cyrena.Extensions;
using Cyrena.Models;
using Cyrena.PlatformIO.Contracts;
using Microsoft.SemanticKernel;
using System.ComponentModel;
using static System.Net.Mime.MediaTypeNames;

namespace Cyrena.PlatformIO.Plugins
{
    internal class Platform
    {
        private readonly IChatMessageService _context;
        private readonly IEnvironmentController _env;
        private readonly IDevelopPlanService _plan;
        public Platform(IChatMessageService context, IEnvironmentController env, IDevelopPlanService plan)
        {
            _context = context;
            _env = env;
            _plan = plan;
        }

        [KernelFunction("get_environment_info")]
        [Description("Gets information about the current PlatformIO environment you are working on.")]
        public Dictionary<string, string?> GetPlatformIOEnvironment()
        {
            if (_env.Current == null)
                return new Dictionary<string, string?>() { { "ERROR", "No active environments" } };
            var model = new Dictionary<string, string?>();
            model["environment"] = _env.Current.Name;
            foreach (var item in _env.Current.Properties)
                model[item.Key] = item.Value;
            return model;
        }

        [KernelFunction("create_h")]
        [Description("Creates a new header file (*.h) in the project's include folder.")]
        public ToolResult<DevelopFile> Create_Include_Header(
            [Description("Name of the file, example 'my_class'.")]string name)
        {
            return CreateFile("include", "h", name);
        }

        [KernelFunction("create_c")]
        [Description("Creates a new c file (*.c) in the project's src folder.")]
        public ToolResult<DevelopFile> CreateCFile(
            [Description("Name of the file, example 'my_class'.")] string name)
        {
            return CreateFile("src", "c", name);
        }

        [KernelFunction("create_cpp")]
        [Description("Creates a new C++ file (*.cpp) in the project's src folder.")]
        public ToolResult<DevelopFile> CreateCppFile(
            [Description("Name of the file, example 'my_class'.")] string name)
        {
            return CreateFile("src", "cpp", name);
        }

        [KernelFunction("create_folder_in_src")]
        [Description("Creates a new folder in the 'src' folder.")]
        public ToolResult<DevelopFolder> CreateSrcFolder(
            [Description("The name of the folder, i.e. 'services'")]string name)
        {
            var src = _plan.Plan.GetOrCreateFolder("src", "src");
            var folder = _plan.Plan.GetOrCreateFolder(src, $"src_{name.ToLower()}", name);
            return new ToolResult<DevelopFolder>(folder);
        }

        [KernelFunction("create_folder_in_include")]
        [Description("Creates a new folder in the 'include' folder.")]
        public ToolResult<DevelopFolder> CreateIncludeFolder(
            [Description("The name of the folder, i.e. 'services'")] string name)
        {
            var include = _plan.Plan.GetOrCreateFolder("include", "include");
            var folder = _plan.Plan.GetOrCreateFolder(include, $"include_{name.ToLower()}", name);
            return new ToolResult<DevelopFolder>(folder);
        }

        [KernelFunction("create_h_in_folder")]
        [Description("Creates a new header file (*.h) in a specific folder in the 'include' folder.")]
        public ToolResult<DevelopFile> Create_Include_Header(
            [Description("Id of the folder")] string folderId,
            [Description("Name of the file, example 'my_class'.")] string name)
        {
            var parent_id = "include";
            var ext = "h";
            name = Path.GetFileNameWithoutExtension(name);
            var parent = _plan.Plan.GetOrCreateFolder(parent_id, parent_id);
            if (!parent.TryFindFolder(folderId, out var target, false))
                return new ToolResult<DevelopFile>(false, $"{folderId} not found in '{parent_id}' folder");
            var id = $"{folderId}_{ext}_{name}";
            if(_plan.Plan.TryFindFile(id, out var file))
                return new ToolResult<DevelopFile>(file!);
            _context.LogInfo($"Creating file {parent_id}/{target!.Name}/{name}.{ext}");
            file = _plan.Plan.CreateFile(target, id, $"{name}.{ext}", null);
            return new ToolResult<DevelopFile>(file);
        }

        [KernelFunction("create_c_in_folder")]
        [Description("Creates a new c file (*.c) in a specific folder in the project's src folder.")]
        public ToolResult<DevelopFile> CreateCFile(
            [Description("Id of the folder")] string folderId,
            [Description("Name of the file, example 'my_class'.")] string name)
        {
            var parent_id = "src";
            var ext = "c";
            name = Path.GetFileNameWithoutExtension(name);
            var parent = _plan.Plan.GetOrCreateFolder(parent_id, parent_id);
            if (!parent.TryFindFolder(folderId, out var target, false))
                return new ToolResult<DevelopFile>(false, $"{folderId} not found in '{parent_id}' folder");
            var id = $"{folderId}_{ext}_{name}";
            if (_plan.Plan.TryFindFile(id, out var file))
                return new ToolResult<DevelopFile>(file!);
            _context.LogInfo($"Creating file {parent_id}/{target!.Name}/{name}.{ext}");
            file = _plan.Plan.CreateFile(target, id, $"{name}.{ext}", null);
            return new ToolResult<DevelopFile>(file);
        }

        [KernelFunction("create_cpp_in_folder")]
        [Description("Creates a new C++ file (*.cpp) in a specific folder in the project's src folder.")]
        public ToolResult<DevelopFile> CreateCppFile(
            [Description("Id of the folder")] string folderId,
            [Description("Name of the file, example 'my_class'.")] string name)
        {
            var parent_id = "src";
            var ext = "cpp";
            name = Path.GetFileNameWithoutExtension(name);
            var parent = _plan.Plan.GetOrCreateFolder(parent_id, parent_id);
            if (!parent.TryFindFolder(folderId, out var target, false))
                return new ToolResult<DevelopFile>(false, $"{folderId} not found in '{parent_id}' folder");
            var id = $"{folderId}_{ext}_{name}";
            if (_plan.Plan.TryFindFile(id, out var file))
                return new ToolResult<DevelopFile>(file!);
            _context.LogInfo($"Creating file {parent_id}/{target!.Name}/{name}.{ext}");
            file = _plan.Plan.CreateFile(target, id, $"{name}.{ext}", null);
            return new ToolResult<DevelopFile>(file);
        }

        private ToolResult<DevelopFile> CreateFile(string dir, string ext, string name)
        {
            name = Path.GetFileNameWithoutExtension(name);
            var id = $"{dir}_{ext}_{name}";
            if (_plan.Plan.TryFindFile(id, out var file))
                return new ToolResult<DevelopFile>(file!, true, "File already exists");
            _context.LogInfo($"Creating file {dir}/{name}.{ext}");
            var target = _plan.Plan.GetOrCreateFolder(dir, dir);
            var nf = _plan.Plan.CreateFile(target, id, $"{name}.{ext}", null);
            return new ToolResult<DevelopFile>(nf);
        }
    }
}
